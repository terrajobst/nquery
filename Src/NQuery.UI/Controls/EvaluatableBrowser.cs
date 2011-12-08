using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using NQuery.Runtime;

using Binding=NQuery.Runtime.Binding;

namespace NQuery.UI
{
	public partial class EvaluatableBrowser : UserControl
	{
		private Evaluatable _evaluatable;
		private DataContext _dataContext;

		private const int FOLDER_IMG_IDX = 0;
		private const int TABLE_IMG_IDX = 1;
		private const int COLUMN_IMG_IDX = 2;
		private const int AGGREGATE_IMG_IDX = 3;
		private const int FUNCTION_IMG_IDX = 4;
		private const int PARAMETER_IMG_IDX = 5;
		private const int RELATION_IMG_IDX = 6;

		public EvaluatableBrowser()
		{
			InitializeComponent();

			treeView.Click += treeView_Click;
			treeView.DoubleClick += treeView_DoubleClick;
		}

		private void treeView_Click(object sender, EventArgs e)
		{
			OnClick(e);
		}

		private void treeView_DoubleClick(object sender, EventArgs e)
		{
			OnDoubleClick(e);
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		public new BorderStyle BorderStyle
		{
			get { return treeView.BorderStyle; }
			set { treeView.BorderStyle = value; }
		}

		public Evaluatable Evaluatable
		{
			get { return _evaluatable; }
			set
			{
				if (_evaluatable != null)
					DisconnectEvaluatable();

				_evaluatable = value;

				if (_evaluatable != null)
					ConnectEvaluatable();

				Reload();
			}
		}

		public object SelectedItem
		{
			get
			{
				if (treeView.SelectedNode == null)
					return null;

				return treeView.SelectedNode.Tag;
			}
		}

		public event EventHandler<EventArgs> SelectedItemChanged;

		private void ConnectEvaluatable()
		{
			_evaluatable.DataContextChanged += Evaluatable_DataContextChanged;
			_evaluatable.Parameters.Changed += Parameters_Changed;
			_dataContext = _evaluatable.DataContext;
			if (_dataContext != null)
				ConnectDataContext();
		}

		private void DisconnectEvaluatable()
		{
			if (_dataContext != null)
				DisconnectDataContext();

			_evaluatable.Parameters.Changed -= Parameters_Changed;
			_evaluatable.DataContextChanged -= Evaluatable_DataContextChanged;
			_evaluatable = null;
		}

		private void ConnectDataContext()
		{
			_dataContext.Changed += DataContext_Changed;
		}

		private void DisconnectDataContext()
		{
			_dataContext.Changed -= DataContext_Changed;
			_dataContext = null;
		}

		private void Evaluatable_DataContextChanged(object sender, EventArgs e)
		{
			if (_dataContext != null)
				DisconnectDataContext();

			_dataContext = _evaluatable.DataContext;

			if (_dataContext != null)
				ConnectDataContext();
		}

		private void DataContext_Changed(object sender, EventArgs e)
		{
			Reload();
		}

		private void Parameters_Changed(object sender, EventArgs e)
		{
			Reload();
		}

		private static TreeNode Add(TreeNodeCollection target, string text, object tag, int imageIndex)
		{
			TreeNode node = new TreeNode();
			node.Text = text;
			node.Tag = tag;
			node.ImageIndex = node.SelectedImageIndex = imageIndex;
			target.Add(node);
			return node;
		}

		private void Reload()
		{
			Dictionary<string, object> expandedNodePaths = new Dictionary<string, object>();

			treeView.BeginUpdate();
			try
			{
				Point scrollPos = NativeMethods.GetScrollPos(treeView);
				SaveExpandedNodes(expandedNodePaths, treeView.Nodes);

				treeView.Nodes.Clear();
				TreeNode folderNode;

				if (_evaluatable != null)
				{
					folderNode = Add(treeView.Nodes, "Parameters", null, FOLDER_IMG_IDX);
					foreach (ParameterBinding parameterBinding in Sorted(_evaluatable.Parameters))
						Add(folderNode.Nodes, parameterBinding.Name, parameterBinding, PARAMETER_IMG_IDX);
				}

				if (_dataContext != null)
				{
					folderNode = Add(treeView.Nodes, "Tables", null, FOLDER_IMG_IDX);
					foreach (TableBinding tableBinding in Sorted(_dataContext.Tables))
					{
						TreeNode tableNode = Add(folderNode.Nodes, tableBinding.Name, tableBinding, TABLE_IMG_IDX);

						TreeNode columnsNode = Add(tableNode.Nodes, "Columns", null, FOLDER_IMG_IDX);
						foreach (ColumnBinding columnBinding in tableBinding.Columns)
							Add(columnsNode.Nodes, columnBinding.Name, columnBinding, COLUMN_IMG_IDX);

						TreeNode parentsNode = Add(tableNode.Nodes, "Parents", null, FOLDER_IMG_IDX);
						foreach (TableRelation tableRelation in _dataContext.TableRelations.GetParentRelations(tableBinding))
							Add(parentsNode.Nodes, tableRelation.ParentTable.Name, tableRelation, RELATION_IMG_IDX);

						TreeNode childrenNode = Add(tableNode.Nodes, "Children", null, FOLDER_IMG_IDX);
						foreach (TableRelation tableRelation in _dataContext.TableRelations.GetChildRelations(tableBinding))
							Add(childrenNode.Nodes, tableRelation.ChildTable.Name, tableRelation, RELATION_IMG_IDX);
					}

					folderNode = Add(treeView.Nodes, "Functions", null, FOLDER_IMG_IDX);
					IEnumerable<List<FunctionBinding>> functionGroups = GetFunctionsGroupedByName(_dataContext.Functions);
					foreach (List<FunctionBinding> functionGroup in functionGroups)
					{
						TreeNode groupNode;

						bool isSingleFunction = functionGroup.Count == 1;
						if (isSingleFunction)
							groupNode = folderNode;
						else
							groupNode = Add(folderNode.Nodes, functionGroup[0].Name, null, FOLDER_IMG_IDX);

						foreach (FunctionBinding functionBinding in functionGroup)
						{
							string functionName;

							if (isSingleFunction)
								functionName = functionBinding.Name;
							else
							{
								StringBuilder sb = new StringBuilder();
								foreach (Type parameterType in functionBinding.GetParameterTypes())
								{
									if (sb.Length > 0)
										sb.Append(", ");
									sb.Append(parameterType.Name);
								}
								functionName = sb.ToString();
							}

							TreeNode functionNode = Add(groupNode.Nodes, functionName, functionBinding, FUNCTION_IMG_IDX);
							foreach (InvokeParameter parameter in functionBinding.GetParameters())
								Add(functionNode.Nodes, parameter.Name + ": " + parameter.DataType.Name, null, PARAMETER_IMG_IDX);
						}
					}

					folderNode = Add(treeView.Nodes, "Aggregates", null, FOLDER_IMG_IDX);
					foreach (AggregateBinding aggregateBinding in Sorted(_dataContext.Aggregates))
						Add(folderNode.Nodes, aggregateBinding.Name, aggregateBinding, AGGREGATE_IMG_IDX);
				}

				RestoreExpandedNodes(expandedNodePaths, treeView.Nodes);
				NativeMethods.SetScrollPos(treeView, scrollPos, false);
			}
			finally
			{
				treeView.EndUpdate();
			}
		}

		private static IEnumerable<T> Sorted<T>(IEnumerable<T> bindings) where T : Binding
		{
			List<T> sorted = new List<T>(bindings);
			sorted.Sort(delegate(T x, T y) { return x.Name.CompareTo(y.Name); });
			return sorted;
		}

		private static IEnumerable<List<FunctionBinding>> GetFunctionsGroupedByName(IEnumerable<FunctionBinding> functions)
		{
			SortedDictionary<string, List<FunctionBinding>> functionGroups = new SortedDictionary<string, List<FunctionBinding>>();
			foreach (FunctionBinding functionBinding in functions)
			{
				List<FunctionBinding> overloads;
				if (!functionGroups.TryGetValue(functionBinding.Name, out overloads))
				{
					overloads = new List<FunctionBinding>();
					functionGroups.Add(functionBinding.Name, overloads);
				}
				overloads.Add(functionBinding);
			}

			foreach (KeyValuePair<string, List<FunctionBinding>> functionGroup in functionGroups)
				yield return functionGroup.Value;
		}

		private static void SaveExpandedNodes(IDictionary<string, object> paths, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.IsExpanded)
					paths.Add(node.FullPath, null);
				SaveExpandedNodes(paths, node.Nodes);
			}
		}

		private static void RestoreExpandedNodes(IDictionary<string, object> paths, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				object dummy;
				if (paths.TryGetValue(node.FullPath, out dummy))
					node.Expand();

				RestoreExpandedNodes(paths, node.Nodes);
			}
		}

		private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			Binding binding =((TreeNode) e.Item).Tag as Binding;
			if (binding != null)
				DoDragDrop(Identifier.CreateNonVerbatim(binding.Name).ToSource(), DragDropEffects.Copy);
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			EventHandler<EventArgs> handler = SelectedItemChanged;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		private void treeView_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				TreeNode treeNode = treeView.GetNodeAt(e.Location);
				if (treeNode != null)
					treeView.SelectedNode = treeNode;
			}
		}
	}
}
