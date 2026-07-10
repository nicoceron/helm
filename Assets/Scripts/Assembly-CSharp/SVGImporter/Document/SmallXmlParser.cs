using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using SVGImporter.Utils;

namespace SVGImporter.Document
{
	public class SmallXmlParser
	{
		public interface IContentHandler
		{
			void OnStartParsing(SmallXmlParser parser);

			void OnStartElement(string name, AttributeList attrs);

			void OnEndElement(string name);

			void OnInlineElement(string name, AttributeList attrs);

			void OnStyleElement(string name, AttributeList attrs, string style);
		}

		private IContentHandler handler;

		private TextReader reader;

		private LiteStack elementNames = new LiteStack();

		private StringBuilder buffer = new StringBuilder(200);

		private char[] nameBuffer = new char[30];

		private AttributeList attributes;

		private int line = 1;

		private int column;

		private bool resetColumn;

		private Exception Error(string msg)
		{
			return new SmallXmlParserException(msg, line, column);
		}

		private Exception UnexpectedEndError()
		{
			string[] array = new string[elementNames.Count];
			(elementNames as ICollection).CopyTo(array, 0);
			return Error(string.Format("Unexpected end of stream. Element stack content is {0}", string.Join(",", array)));
		}

		private bool IsNameChar(char c, bool start)
		{
			switch (c)
			{
			case ':':
			case '_':
				return true;
			case '-':
			case '.':
				return !start;
			default:
				if (c > 'Ā')
				{
					if (c == 'ՙ' || c == 'ۥ' || c == 'ۦ')
					{
						return true;
					}
					if ('ʻ' <= c && c <= 'ˁ')
					{
						return true;
					}
				}
				switch (char.GetUnicodeCategory(c))
				{
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.OtherLetter:
				case UnicodeCategory.LetterNumber:
					return true;
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.NonSpacingMark:
				case UnicodeCategory.SpacingCombiningMark:
				case UnicodeCategory.EnclosingMark:
				case UnicodeCategory.DecimalDigitNumber:
					return !start;
				default:
					return false;
				}
			}
		}

		private bool IsWhitespace(int c)
		{
			if ((uint)(c - 9) <= 1u || c == 13 || c == 32)
			{
				return true;
			}
			return false;
		}

		public void SkipWhitespaces()
		{
			SkipWhitespaces(expected: false);
		}

		private void HandleWhitespaces()
		{
			while (IsWhitespace(Peek()))
			{
				buffer.Append((char)Read());
			}
		}

		public void SkipWhitespaces(bool expected)
		{
			while (true)
			{
				int num = Peek();
				if ((uint)(num - 9) > 1u && num != 13 && num != 32)
				{
					break;
				}
				Read();
				if (expected)
				{
					expected = false;
				}
			}
			if (expected)
			{
				throw Error("Whitespace is expected.");
			}
		}

		private int Peek()
		{
			return reader.Peek();
		}

		private int Read()
		{
			int num = reader.Read();
			if (num == 10)
			{
				resetColumn = true;
			}
			if (resetColumn)
			{
				line++;
				resetColumn = false;
				column = 1;
				return num;
			}
			column++;
			return num;
		}

		public void Expect(int c)
		{
			int num = Read();
			if (num < 0)
			{
				throw UnexpectedEndError();
			}
			if (num != c)
			{
				throw Error($"Expected '{(char)c}' but got {(char)num}");
			}
		}

		private string ReadUntil(char until, bool handleReferences)
		{
			while (true)
			{
				if (Peek() < 0)
				{
					throw UnexpectedEndError();
				}
				char c = (char)Read();
				if (c == until)
				{
					break;
				}
				if (handleReferences && c == '&')
				{
					ReadReference();
				}
				else
				{
					buffer.Append(c);
				}
			}
			string result = buffer.ToString();
			buffer.Length = 0;
			return result;
		}

		public string ReadName()
		{
			int num = 0;
			if (Peek() < 0 || !IsNameChar((char)Peek(), start: true))
			{
				throw Error("XML name start character is expected.");
			}
			for (int num2 = Peek(); num2 >= 0; num2 = Peek())
			{
				char c = (char)num2;
				if (!IsNameChar(c, start: false))
				{
					break;
				}
				if (num == nameBuffer.Length)
				{
					char[] destinationArray = new char[num * 2];
					Array.Copy(nameBuffer, 0, destinationArray, 0, num);
					nameBuffer = destinationArray;
				}
				nameBuffer[num++] = c;
				Read();
			}
			if (num == 0)
			{
				throw Error("Valid XML name is expected.");
			}
			return new string(nameBuffer, 0, num);
		}

		public void Parse(TextReader input, IContentHandler handler)
		{
			reader = input;
			this.handler = handler;
			handler.OnStartParsing(this);
			while (Peek() >= 0)
			{
				ReadContent();
			}
			buffer.Length = 0;
			if (elementNames.Count > 0)
			{
				throw Error($"Insufficient close tag: {elementNames.Peek()}");
			}
			Cleanup();
		}

		private void Cleanup()
		{
			line = 1;
			column = 0;
			handler = null;
			reader = null;
			elementNames.Clear();
			attributes.Clear();
			buffer.Length = 0;
		}

		public void ReadContent()
		{
			if (IsWhitespace(Peek()))
			{
				HandleWhitespaces();
			}
			if (Peek() == 60)
			{
				Read();
				string text;
				switch (Peek())
				{
				case 33:
					Read();
					if (Peek() == 91)
					{
						Read();
						if (ReadName() != "CDATA")
						{
							throw Error("Invalid declaration markup");
						}
						Expect(91);
						ReadCDATASection();
					}
					else if (Peek() == 45)
					{
						ReadComment();
					}
					else
					{
						if (ReadName() != "DOCTYPE")
						{
							throw Error("Invalid declaration markup.");
						}
						ReadUntil('>', handleReferences: false);
					}
					return;
				case 63:
				{
					buffer.Length = 0;
					Read();
					text = ReadName();
					SkipWhitespaces();
					string text3 = string.Empty;
					if (Peek() != 63)
					{
						while (true)
						{
							text3 += ReadUntil('?', handleReferences: false);
							if (Peek() == 62)
							{
								break;
							}
							text3 += "?";
						}
					}
					Expect(62);
					return;
				}
				case 47:
				{
					buffer.Length = 0;
					if (elementNames.Count == 0)
					{
						throw UnexpectedEndError();
					}
					Read();
					text = ReadName();
					SkipWhitespaces();
					string text2 = (string)elementNames.Pop();
					if (text != text2)
					{
						throw Error($"End tag mismatch: expected {text2} but found {text}");
					}
					handler.OnEndElement(text);
					Expect(62);
					return;
				}
				}
				buffer.Length = 0;
				text = ReadName();
				string text4 = text.ToLower();
				if (text4 != null && text4 == "style")
				{
					while (Peek() != 62 && Peek() != 47)
					{
						ReadAttribute(ref attributes);
					}
					SkipWhitespaces();
					if (Peek() == 47)
					{
						Read();
					}
					ReadUntil('>', handleReferences: false);
					string cssString = ReadUntil('<', handleReferences: false);
					cssString = CSSParser.CleanCSS(cssString);
					if (string.IsNullOrEmpty(cssString))
					{
						cssString = ReadUntil('<', handleReferences: false);
					}
					if (!string.IsNullOrEmpty(cssString))
					{
						cssString = cssString.Replace("![CDATA[", "");
						cssString = cssString.Replace("]]>", "");
						cssString = CSSParser.CleanCSS(cssString);
					}
					handler.OnStyleElement(text, attributes, cssString);
					attributes.Clear();
					ReadUntil('>', handleReferences: false);
				}
				else
				{
					while (Peek() != 62 && Peek() != 47)
					{
						ReadAttribute(ref attributes);
					}
					SkipWhitespaces();
					if (Peek() == 47)
					{
						handler.OnInlineElement(text, attributes);
						Read();
					}
					else
					{
						handler.OnStartElement(text, attributes);
						elementNames.Push(text);
					}
					attributes.Clear();
					Expect(62);
				}
			}
			else
			{
				ReadCharacters();
			}
		}

		private void ReadCharacters()
		{
			while (true)
			{
				switch (Peek())
				{
				case -1:
				case 60:
					return;
				case 38:
					Read();
					ReadReference();
					break;
				default:
					buffer.Append((char)Read());
					break;
				}
			}
		}

		private void ReadReference()
		{
			if (Peek() == 35)
			{
				Read();
				ReadCharacterReference();
				return;
			}
			string text = ReadName();
			Expect(59);
			switch (text)
			{
			case "amp":
				buffer.Append('&');
				break;
			case "quot":
				buffer.Append('"');
				break;
			case "apos":
				buffer.Append('\'');
				break;
			case "lt":
				buffer.Append('<');
				break;
			case "gt":
				buffer.Append('>');
				break;
			default:
				throw Error("General non-predefined entity reference is not supported in this parser.");
			}
		}

		private int ReadCharacterReference()
		{
			int num = 0;
			if (Peek() == 120)
			{
				Read();
				for (int num2 = Peek(); num2 >= 0; num2 = Peek())
				{
					if (48 <= num2 && num2 <= 57)
					{
						num <<= 4 + num2 - 48;
					}
					else if (65 <= num2 && num2 <= 70)
					{
						num <<= 4 + num2 - 65 + 10;
					}
					else
					{
						if (97 > num2 || num2 > 102)
						{
							break;
						}
						num <<= 4 + num2 - 97 + 10;
					}
					Read();
				}
			}
			else
			{
				int num3 = Peek();
				while (num3 >= 0 && 48 <= num3 && num3 <= 57)
				{
					num <<= 4 + num3 - 48;
					Read();
					num3 = Peek();
				}
			}
			return num;
		}

		private void ReadAttribute(ref AttributeList a)
		{
			SkipWhitespaces(expected: true);
			if (Peek() != 47 && Peek() != 62)
			{
				string name = ReadName();
				SkipWhitespaces();
				Expect(61);
				SkipWhitespaces();
				a.Add(name, Read() switch
				{
					39 => ReadUntil('\'', handleReferences: true), 
					34 => ReadUntil('"', handleReferences: true), 
					_ => throw Error("Invalid attribute value markup."), 
				});
			}
		}

		private void ReadCDATASection()
		{
			int num = 0;
			while (Peek() >= 0)
			{
				char c = (char)Read();
				switch (c)
				{
				case ']':
					num++;
					continue;
				case '>':
					if (num > 1)
					{
						for (int num2 = num; num2 > 2; num2--)
						{
							buffer.Append(']');
						}
						return;
					}
					break;
				}
				for (int i = 0; i < num; i++)
				{
					buffer.Append(']');
				}
				num = 0;
				buffer.Append(c);
			}
			throw UnexpectedEndError();
		}

		private void ReadComment()
		{
			Expect(45);
			Expect(45);
			while (Read() != 45 || Read() != 45)
			{
			}
			if (Read() != 62)
			{
				throw Error("'--' is not allowed inside comment markup.");
			}
		}
	}
}
