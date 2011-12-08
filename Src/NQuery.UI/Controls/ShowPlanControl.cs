using System;
using System.Globalization;
using System.Windows.Forms;

namespace NQuery.UI
{
	public partial class ShowPlanControl : UserControl
	{
		private ShowPlan _showPlan;

		public ShowPlanControl()
		{
			InitializeComponent();

			NativeMethods.SetOverlayImage(executionPlanImages, 11, 1);
		}

		public ShowPlan ShowPlan
		{
			get { return _showPlan; }
			set
			{
				_showPlan = value;
				LoadQueryPlan(_showPlan);
			}
		}

		public override bool Focused
		{
			get
			{
				return base.Focused || showPlanTreeView.Focused;
			}
		}

		public ShowPlanElement SelectedElement
		{
			get { return (ShowPlanElement) showPlanTreeView.SelectedNode.Tag; }
		}

		public event EventHandler<EventArgs> SelectedElementChanged;

		protected virtual void OnSelectedShowPlanElementChanged()
		{
			EventHandler<EventArgs> eventHandler = SelectedElementChanged;
			if (eventHandler != null)
				eventHandler(this, EventArgs.Empty);
		}

		private void LoadQueryPlan(ShowPlan plan)
		{
			showPlanTreeView.BeginUpdate();
			try
			{
				showPlanTreeView.Nodes.Clear();

				if (plan != null)
				{
					AddQueryPlanElement(showPlanTreeView.Nodes, plan.Root);
					showPlanTreeView.ExpandAll();
				}
			}
			finally
			{
				showPlanTreeView.EndUpdate();
			}
		}

		private static void AddQueryPlanElement(TreeNodeCollection target, ShowPlanElement element)
		{
			const string LOGICAL_OPERATOR_KEY = "Logical Operator";
			const string TABLE_NAME_KEY = "Table";
			const string WITH_TIES_KEY = "With Ties";
			const string WARNING_KEY = "Warning";

			const int HASH_MATCH_IMG_IDX = 0;
			const int NESTED_LOOPS_IMG_IDX = 1;
			const int COMPUTE_SCALAR_IMG_IDX = 2;
			const int FILTER_IMG_IDX = 3;
			const int SORT_IMG_IDX = 4;
			const int STREAM_AGGREGATE_IMG_IDX = 5;
			const int TOP_IMG_IDX = 6;
			const int TABLE_SCAN_IMG_IDX = 7;
			const int SELECT_IMG_IDX = 8;
			const int CONCATENATION_IMG_IDX = 9;
			const int CONSTANT_SCAN_IMG_IDX = 10;
			const int ASSERT_IMG_IDX = 12;
			const int INDEX_SPOOL_IMG_IDX = 13;
			const int TABLE_SPOOL_IMG_IDX = 14;

			int imageIndex;
			string nodeDetails = null;

			switch (element.Operator)
			{
				case ShowPlanOperator.Select:
					imageIndex = SELECT_IMG_IDX;
					break;
				case ShowPlanOperator.TableScan:
					imageIndex = TABLE_SCAN_IMG_IDX;
					nodeDetails = element.Properties[TABLE_NAME_KEY].Value;
					break;
				case ShowPlanOperator.NestedLoops:
					imageIndex = NESTED_LOOPS_IMG_IDX;
					nodeDetails = element.Properties[LOGICAL_OPERATOR_KEY].Value;
					break;
				case ShowPlanOperator.ConstantScan:
					imageIndex = CONSTANT_SCAN_IMG_IDX;
					break;
				case ShowPlanOperator.ComputeScalar:
					imageIndex = COMPUTE_SCALAR_IMG_IDX;
					break;
				case ShowPlanOperator.Concatenation:
					imageIndex = CONCATENATION_IMG_IDX;
					break;
				case ShowPlanOperator.Sort:
					imageIndex = SORT_IMG_IDX;
					nodeDetails = element.Properties[LOGICAL_OPERATOR_KEY].Value;
					break;
				case ShowPlanOperator.StreamAggregate:
					imageIndex = STREAM_AGGREGATE_IMG_IDX;
					break;
				case ShowPlanOperator.Top:
					imageIndex = TOP_IMG_IDX;
					if (element.Properties[WITH_TIES_KEY].Value == Boolean.TrueString)
						nodeDetails = "With Ties";
					break;
				case ShowPlanOperator.Filter:
					imageIndex = FILTER_IMG_IDX;
					break;
				case ShowPlanOperator.Assert:
					imageIndex = ASSERT_IMG_IDX;
					break;
				case ShowPlanOperator.TableSpool:
					imageIndex = TABLE_SPOOL_IMG_IDX;
					nodeDetails = element.Properties[LOGICAL_OPERATOR_KEY].Value;
					break;
				case ShowPlanOperator.IndexSpool:
					imageIndex = INDEX_SPOOL_IMG_IDX;
					nodeDetails = element.Properties[LOGICAL_OPERATOR_KEY].Value;
					break;
				case ShowPlanOperator.HashMatch:
					imageIndex = HASH_MATCH_IMG_IDX;
					nodeDetails = element.Properties[LOGICAL_OPERATOR_KEY].Value;
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(element.Operator);
			}

			string nodeName;

			if (nodeDetails == null)
				nodeName = element.Operator.ToString();
			else
				nodeName = String.Format(CultureInfo.CurrentCulture, "{0} ({1})", element.Operator, nodeDetails);

			TreeNode node = new TreeNode();
			node.Text = nodeName;
			node.ImageIndex = node.SelectedImageIndex = imageIndex;
			node.Tag = element;
			target.Add(node);

			if (element.Properties.Contains(WARNING_KEY))
				NativeMethods.SetOverlayIndex(node, 1);

			foreach (ShowPlanElement child in element.Children)
				AddQueryPlanElement(node.Nodes, child);
		}

		private void showPlanTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			OnSelectedShowPlanElementChanged();
		}
	}
}
