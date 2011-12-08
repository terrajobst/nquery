using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Tests
{
	[TestClass]
	public sealed class IdentifierTests : AutomatedTestFixtureBase
	{
		[TestMethod]
		public void FromSourceParenthesized()
		{
			Identifier identifier;
			
			identifier = Identifier.FromSource("[Test]");
			Assert.AreEqual("Test", identifier.Text);
			Assert.AreEqual("[Test]", identifier.ToSource());
			Assert.IsTrue(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.FromSource("[Test 123 test]");
			Assert.AreEqual("Test 123 test", identifier.Text);
			Assert.AreEqual("[Test 123 test]", identifier.ToSource());
			Assert.IsTrue(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.FromSource("[Test\t123\ntest]");
			Assert.AreEqual("Test\t123\ntest", identifier.Text);
			Assert.AreEqual("[Test\t123\ntest]", identifier.ToSource());
			Assert.IsTrue(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);
		}

		[TestMethod]
		public void FormSourceQuoted()
		{
			Identifier identifier;
			
			identifier = Identifier.FromSource("\"Test\"");
			Assert.AreEqual("Test", identifier.Text);
			Assert.AreEqual("\"Test\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.FromSource("\"Test 123 test\"");
			Assert.AreEqual("Test 123 test", identifier.Text);
			Assert.AreEqual("\"Test 123 test\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.FromSource("\"Test\t123\ntest\"");
			Assert.AreEqual("Test\t123\ntest", identifier.Text);
			Assert.AreEqual("\"Test\t123\ntest\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);
		}

		[TestMethod]
		public void FromSourceParenthesizedAndEscaped()
		{
			Identifier identifier;
			
			identifier = Identifier.FromSource("[Test]]]");
			Assert.AreEqual("Test]", identifier.Text);
			Assert.AreEqual("[Test]]]", identifier.ToSource());
			Assert.IsTrue(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.FromSource("[Test]]xx]");
			Assert.AreEqual("Test]xx", identifier.Text);
			Assert.AreEqual("[Test]]xx]", identifier.ToSource());
			Assert.IsTrue(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);
		}

		[TestMethod]
		public void FormSourceQuotedAndEscaped()
		{
			Identifier identifier;
			
			identifier = Identifier.FromSource("\"Test\"\"\"");
			Assert.AreEqual("Test\"", identifier.Text);
			Assert.AreEqual("\"Test\"\"\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.FromSource("\"Test\"\"xx\"");
			Assert.AreEqual("Test\"xx", identifier.Text);
			Assert.AreEqual("\"Test\"\"xx\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);
		}

		[TestMethod]
		public void SimpleIsNotParenthesized()
		{
			Identifier identifier;

			identifier = Identifier.CreateNonVerbatim("Identifier");
			Assert.AreEqual("Identifier", identifier.Text);
			Assert.AreEqual("Identifier", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.CreateNonVerbatim("_Identifier");
			Assert.AreEqual("_Identifier", identifier.Text);
			Assert.AreEqual("_Identifier", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.CreateNonVerbatim("This_is_a_valid_identifier");
			Assert.AreEqual("This_is_a_valid_identifier", identifier.Text);
			Assert.AreEqual("This_is_a_valid_identifier", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.CreateNonVerbatim("Identifier1");
			Assert.AreEqual("Identifier1", identifier.Text);
			Assert.AreEqual("Identifier1", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.CreateNonVerbatim("My$Dollars");
			Assert.AreEqual("My$Dollars", identifier.Text);
			Assert.AreEqual("My$Dollars", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.CreateNonVerbatim("This_is_a_valid$_identifier_42_$");
			Assert.AreEqual("This_is_a_valid$_identifier_42_$", identifier.Text);
			Assert.AreEqual("This_is_a_valid$_identifier_42_$", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);
		}

		[TestMethod]
		public void SimpleRemainsQuoted()
		{
			Identifier identifier;

			identifier = Identifier.CreateVerbatim("Identifier");
			Assert.AreEqual("Identifier", identifier.Text);
			Assert.AreEqual("\"Identifier\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.CreateVerbatim("_Identifier");
			Assert.AreEqual("_Identifier", identifier.Text);
			Assert.AreEqual("\"_Identifier\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.CreateVerbatim("This_is_a_valid_identifier");
			Assert.AreEqual("This_is_a_valid_identifier", identifier.Text);
			Assert.AreEqual("\"This_is_a_valid_identifier\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.CreateVerbatim("Identifier1");
			Assert.AreEqual("Identifier1", identifier.Text);
			Assert.AreEqual("\"Identifier1\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.CreateVerbatim("My$Dollars");
			Assert.AreEqual("My$Dollars", identifier.Text);
			Assert.AreEqual("\"My$Dollars\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.CreateVerbatim("This_is_a_valid$_identifier_42_$");
			Assert.AreEqual("This_is_a_valid$_identifier_42_$", identifier.Text);
			Assert.AreEqual("\"This_is_a_valid$_identifier_42_$\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);
		}

		[TestMethod]
		public void KeywordsAreParenthesized()
		{
			Identifier identifier;

			identifier = Identifier.CreateNonVerbatim("SELECT");
			Assert.AreEqual("SELECT", identifier.Text);
			Assert.AreEqual("[SELECT]", identifier.ToSource());
			Assert.IsTrue(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);

			identifier = Identifier.CreateNonVerbatim("Top");
			Assert.AreEqual("Top", identifier.Text);
			Assert.AreEqual("[Top]", identifier.ToSource());
			Assert.IsTrue(identifier.Parenthesized);
			Assert.IsFalse(identifier.Verbatim);
		}

		[TestMethod]
		public void KeywordsRemainsQuoted()
		{
			Identifier identifier;

			identifier = Identifier.CreateVerbatim("SELECT");
			Assert.AreEqual("SELECT", identifier.Text);
			Assert.AreEqual("\"SELECT\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);

			identifier = Identifier.CreateVerbatim("Top");
			Assert.AreEqual("Top", identifier.Text);
			Assert.AreEqual("\"Top\"", identifier.ToSource());
			Assert.IsFalse(identifier.Parenthesized);
			Assert.IsTrue(identifier.Verbatim);
		}

		[TestMethod]
		public void MatchingWorks()
		{
			Identifier a1 = Identifier.CreateVerbatim("Name");
			Identifier a2 = Identifier.CreateVerbatim("NAME");

			Identifier b1 = Identifier.CreateNonVerbatim("Name");
			Identifier b2 = Identifier.CreateNonVerbatim("NAME");

			// Every identifier matches itself

			Assert.IsTrue(a1.Matches(a1));
			Assert.IsTrue(a2.Matches(a2));
			Assert.IsTrue(b1.Matches(b1));
			Assert.IsTrue(b2.Matches(b2));

			// Verbatim idenfiers are compared case sensitive

			Assert.IsFalse(a1.Matches(a2));
			Assert.IsFalse(a2.Matches(a1));

			// Non verbatim identifiers are compared case insensitive

			Assert.IsTrue(b1.Matches(b2));
			Assert.IsTrue(b2.Matches(b1));

			// A verbatim identifier compared with a non-verbatim identifier will never produce a match

			Assert.IsFalse(a1.Matches(b1));
			Assert.IsFalse(a2.Matches(b2));

			// A non-verbatim identifier and a verbatim identifier are compared case insensitive

			Assert.IsTrue(b1.Matches(a1));
			Assert.IsTrue(b1.Matches(a2));
			Assert.IsTrue(b2.Matches(a1));
			Assert.IsTrue(b2.Matches(a2));
		}

		[TestMethod]
		public void EqualityWorks()
		{
			Identifier a1 = Identifier.CreateVerbatim("Name");
			Identifier a2 = Identifier.CreateVerbatim("NAME");

			Identifier b1 = Identifier.CreateNonVerbatim("Name");
			Identifier b2 = Identifier.CreateNonVerbatim("NAME");

			// Every identifier equals itself

#pragma warning disable 1718 // Comparison made to same variable; did you mean to compare something else?
			Assert.IsTrue(a1 == a1);
			Assert.IsTrue(a2 == a2);
			Assert.IsTrue(b1 == b1);
			Assert.IsTrue(b2 == b2);
#pragma warning restore 1718

			// Equality is done case sensitive. Thefore both the verbatim and non-verbatim version
			// should not be equal to each other.

			Assert.IsFalse(a1 == a2);
			Assert.IsFalse(a1 == b1);
			Assert.IsFalse(a1 == b2);

			Assert.IsFalse(a2 == a1);
			Assert.IsFalse(a1 == b1);
			Assert.IsFalse(a1 == b2);

			Assert.IsFalse(b1 == a1);
			Assert.IsFalse(b1 == a2);
			Assert.IsFalse(b1 == b2);

			Assert.IsFalse(b2 == a1);
			Assert.IsFalse(b2 == a2);
			Assert.IsFalse(b2 == b1);
		}

		[TestMethod]
		public void LexerAcceptsAllTypesOfIdentifiers()
		{
			RunTestOfCallingMethod();
		}
	}
}
