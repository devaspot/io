        
using System;
using System.Collections;

namespace io {

	public class IoLexer {

		public static string specialChars = ":._";
		public string s;
        public int currentPos;
		public char current { get { return s[currentPos]; } }
		public ArrayList charLineIndex = new ArrayList();
		public long lineHint;
		public long maxChar;
		public Stack posStack = new Stack();
		public Stack tokenStack = new Stack();
		public ArrayList tokenStream = new ArrayList();
		public int resultIndex = 0;
		public IoToken errorToken;
		public string errorDescription;

		public void print()
		{
			IoToken first = tokenStream[0] as IoToken;
			if (first != null)
			{
				first.print();
			}
			Console.WriteLine();
		}

		public void printTokens()
		{
			int i;
			for (i = 0; i < tokenStream.Count; i ++)
			{
				IoToken t = tokenStream[i] as IoToken;
				Console.Write("'{0}' {1}", t.name, t.typeName());
				if (i < tokenStream.Count - 1)
				{
					Console.Write(", ");
				}
			}
			Console.WriteLine();
		}


		public int lex()
		{
			pushPos();
			messageChain();

			if (!onNULL())
			{

				if (errorToken == null)
				{
					if (tokenStream.Count != 0)
					{
						errorToken = currentToken();
					}
					else
					{
						errorToken = addTokenStringType(s.Substring(currentPos, 30), IoTokenType.NO_TOKEN);
					}

					errorToken.error = "Syntax error near this location";
				}
				return -1;
			}
			return 0;
		}

        public IoToken top()
        {
            if (resultIndex >= tokenStream.Count) return null;
            return tokenStream[resultIndex] as IoToken;
        }

		public int lastPos() 
		{
			return Convert.ToInt32(posStack.Peek());
		}

		public void pushPos()
		{
            tokenStack.Push(tokenStream.Count-1);
			posStack.Push(currentPos);
		}

		public void popPos()
		{
			tokenStack.Pop();
            posStack.Pop();
		}

        public IoTokenType topType()
        {
            if (top() == null) return 0;
            return top().type;
        }

        public IoToken pop()
        {
            IoToken first = top();
            resultIndex++;
            return first;
        }

		public void popPosBack()
		{
			int i = Convert.ToInt32(tokenStack.Pop());
			int topIndex = Convert.ToInt32(tokenStack.Peek());
            if (i > -1)
            {
                if (i != topIndex)
                {
                    IoToken parent = currentToken();
                    if (parent != null)
                    {
                        parent.nextToken = null;
                    }
                }
            }
			currentPos = Convert.ToInt32(posStack.Pop());
		}

		public char nextChar()
		{
            if (currentPos >= s.Length)
                return '\0';
			char c = current;
			currentPos++;
			return c;
		}

        public char peekChar()
        {
            if (currentPos >= s.Length)
                return '\0';
            char c = current;
            return c;
        }

		public char prevChar()
		{
			currentPos--;
			char c = current;
			return c;
		}

		public int readPadding()
		{

			int r = 0;
			while (readWhitespace() != 0|| readComment() != 0)
			{
				r = 1;
			}
			return r;
		}

		// grabbing

		public int grabLength()
		{
			int i1 = lastPos();
			int i2 = currentPos;
			return i2 - i1;
		}

		public IoToken grabTokenType(IoTokenType type)
		{
			int len = grabLength();

			string s1 = s.Substring(lastPos(), len);

			if (len == 0)
			{
				throw new Exception("IoLexer fatal error: empty token\n");
			}

			return addTokenStringType(s1, type);
		}

		public int currentLineNumber()
		{
			return 0;
		}

		public IoToken addTokenStringType(string s1, IoTokenType type)
		{
		
            IoToken top = currentToken();
        	IoToken t = new IoToken();

			t.lineNumber = currentLineNumber();
			t.charNumber = currentPos;

			if (t.charNumber < 0)
			{
				System.Console.WriteLine("bad t->charNumber = %i\n", t.charNumber);
			}

			t.name = s1;
			t.type = type;

			if (top != null)
			{
				top.nextToken = t;
			}

			tokenStream.Add(t);
			return t;
		}

      	public IoToken currentToken()
        {
            if (tokenStream.Count == 0) return null;
        	return tokenStream[tokenStream.Count-1] as IoToken;
        }

        // message chain

		public void messageChain()
		{
			do {
				while (	readTerminator() != 0 || readSeparator() != 0|| readComment()!=0);
			} while (readMessage()!=0);
		}

		// symbols

		public int readSymbol()
		{
			if (readNumber() != 0 || readOperator() != 0 || readIdentifier() != 0 || readQuote() != 0) return 1;
			return 0;
		}

		public int readIdentifier()
		{
			pushPos();
			while (readLetter() != 0 || readDigit() != 0 || readSpecialChar() != 0);
			if (grabLength() != 0)
			{
				if (s[currentPos - 1] == ':' && s[currentPos] == '=') prevChar();
				grabTokenType(IoTokenType.IDENTIFIER_TOKEN);
				popPos();
				return 1;
			}
 			return 0;
		}

		public int readOperator()
		{
			char c;
			pushPos();
			c = nextChar();
			if (c == 0)
			{
				popPosBack();
				return 0;
			} else {
				prevChar();
			}

			while (readOpChar() != 0);

			if (grabLength() != 0)
			{
				grabTokenType(IoTokenType.IDENTIFIER_TOKEN);
				popPos();
				return 1;

			}

			popPosBack();
			return 0;

		}

		public bool onNULL() 
		{
			return currentPos == s.Length;
		}

		// helpers

		public int readTokenCharsType(string chars, IoTokenType type)
		{
			foreach (char c in chars)
			{
				if (readTokenCharType(c, type) != 0) 
					return 1;
			}
			return 0;
		}

		public int readTokenCharType(char c, IoTokenType type)
		{
			pushPos();

			if (readChar(c) != 0)
			{
				grabTokenType(type);
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

		public int readTokenString(string s)
		{
			pushPos();

			if (readString(s) != 0)
			{
				grabTokenType(IoTokenType.IDENTIFIER_TOKEN);
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

		public int readString(string str)
		{
			int len = str.Length;
            if (len > s.Length - currentPos)
                len = s.Length - currentPos;
			if (onNULL())
			{
				return 0;
			}

			string inmem = s.Substring(currentPos, len);

			if (inmem.Equals(str))
			{
				currentPos += len;
				return 1;
			}

			return 0;
		}

		public int readCharIn(string s)
		{
			if (!onNULL())
			{
				char c = nextChar();
				if (s.IndexOf(c) != -1)
				{
					return 1;
				}
				prevChar();
			}
			return 0;
		}

		public int readCharInRange(char first, char last)
		{
			if (!onNULL())
			{
				char c = nextChar();
				if ((int)c >= (int)first && (int)c <= (int)last) {
					return 1;
				}
				prevChar();
			}
			return 0;
		}

        public int readNonASCIIChar()
        {
            if (!onNULL())
            {
                char nc = nextChar();

                if (nc >= 0x80)
                    return 1;

                prevChar();
            }
            return 0;
        }

		public int readChar(char ch)
		{
			if (!onNULL())
			{
				char c = nextChar();
				if (c == ch) {
					return 1;
				}
				prevChar();
			}
			return 0;
		}

		public int readCharAnyCase(char ch)
		{
			if (!onNULL())
			{
				char c = nextChar();
				if (Char.ToLower(c) == Char.ToLower(ch)) {
					return 1;
				}
				prevChar();
			}
			return 0;
		}

		public bool readNonReturn()
		{
			if (onNULL()) return false;
			if (nextChar() != '\n') return true;
			prevChar();
			return false;
		}

		public bool readNonQuote()
		{
			if (onNULL()) return false;
			if (nextChar() != '"') return true;
			prevChar();
			return false;
		}

		// character definitions

		public int readCharacters()
		{
			int read = 0;
			while (readCharacter() != 0)
			{       	
				read = 1;
			}
			return read;
		}

		public int readCharacter()
		{
			return Convert.ToInt32(readLetter() != 0 || readDigit() != 0 || readSpecialChar() != 0 || readOpChar() != 0);
		}

		public int readOpChar()
		{
			return readCharIn(":'~!@$%^&*-+=|\\<>?/");
		}

		public int readSpecialChar()
		{
			return readCharIn(IoLexer.specialChars);
		}

		public int readDigit()
		{
			return readCharInRange('0', '9');
		}

		public int readLetter() // grab all symbols
		{
            return Convert.ToInt32(readCharInRange('A', 'Z') !=0|| readCharInRange('a', 'z')!=0
                || readNonASCIIChar()!=0);
            /*
			if (!onNULL())
			{
				char c = nextChar();
				return 1;
			}
             */
			//return 0;
		}

		// comments

		public int readComment()
		{
			///return 0;
			//return (readSlashStarComment() || readSlashSlashComment() || readPoundComment());
			return readSlashSlashComment();
		}

		private int readSlashSlashComment()
		{
			this.pushPos();
			if (nextChar() == '/')
			{
				if (nextChar() == '/')
				{
					while (readNonReturn()) { }
					popPos();
					return 1;
				}
			}
			popPosBack();
			return 0;
		}

		// quotes

		public int readQuote()
		{
            return Convert.ToInt32(readTriQuote() != 0 || readMonoQuote() != 0);
		}

		public int readMonoQuote()
		{
			pushPos();

			if (nextChar() == '"')
			{
				while (true)
				{
					char c = nextChar();

					if (c == '"')
					{
						break;
					}

					if (c == '\\')
					{
						nextChar();
						continue;
					}

					if (c == 0)
					{
						errorToken = currentToken();

						if (errorToken != null)
						{
							errorToken.error = "unterminated quote";
						}

						popPosBack();
						return 0;
					}
				}

				grabTokenType(IoTokenType.MONOQUOTE_TOKEN);
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

		public int readTriQuote()
		{
			pushPos();

			if (readString("\"\"\"") != 0)
			{
				while (readString("\"\"\"") == 0)
				{
					char c = nextChar();

					if (c == 0)
					{
						popPosBack();
						return 0;
					}
				}
	
				grabTokenType(IoTokenType.TRIQUOTE_TOKEN);
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

		// terminators

		public int readTerminator()
		{
			int terminated = 0;
			pushPos();
			readSeparator();

			while (readTerminatorChar() != 0)
			{
				terminated = 1;
				readSeparator();
			}

			if (terminated != 0)
			{
				IoToken top = currentToken();

				// avoid double terminators
				if (top != null && top.type == IoTokenType.TERMINATOR_TOKEN)
				{
					return 1;
				}

				addTokenStringType(";", IoTokenType.TERMINATOR_TOKEN);
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

		public int readTerminatorChar()
		{
			return readCharIn(";\n");
		}

		// separators

		public int readSeparator()
		{
			pushPos();

			while (readSeparatorChar() != 0);

			if (grabLength() != 0)
			{
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

		public int readSeparatorChar()
		{
			if (readCharIn(" \f\r\t\v") != 0)
			{
				return 1;
			}
			else
			{
				pushPos();
				if (readCharIn("\\") != 0)
				{
					while (readCharIn(" \f\r\t\v") != 0);
					if (readCharIn("\n") != 0)
					{
						popPos();
						return 1;
					}
				}
				popPosBack();
				return 0;
			}
		}

		// whitespace

		int readWhitespace()
		{
			pushPos();

			while (readWhitespaceChar() != 0);

			if (grabLength() != 0)
			{
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

		public int readWhitespaceChar()
		{
			return readCharIn(" \f\r\t\v\n");
		}

		///

		public int readDigits()
		{
			int read = 0;
			pushPos();

			while (readDigit() != 0)
			{
				read = 1;
			}

			if (read == 0)
			{
				popPosBack();
				return 0;
			}

			popPos();
			return read;
		}

		public int readNumber()
		{
			return Convert.ToInt32(readHexNumber() != 0 || readDecimal() != 0);
		}

		public int readExponent()
		{
			if (readCharAnyCase('e') != 0)
			{
				if (readChar('-') !=0 || readChar('+')!=0) { }

				if (readDigits() == 0)
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}

		public int readDecimalPlaces()
		{
			if (readChar('.')!=0)
			{
				if (readDigits() == 0)
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}


		public int readDecimal()
		{
			pushPos();

			if (readDigits()!=0)
			{
				if (readDecimalPlaces() == -1)
				{
					goto error;
				}
			}
			else
			{
				if (readDecimalPlaces() != 1)
				{
					goto error;
				}
			}

			if (readExponent() == -1)
			{
				goto error;
			}

			if (grabLength()!=0)
			{
				grabTokenType(IoTokenType.NUMBER_TOKEN);
				popPos();
				return 1;
			}
error:
			popPosBack();
			return 0;
		}

		public int readHexNumber()
		{
			int read = 0;
			pushPos();

			if (readChar('0') != 0 && readCharAnyCase('x') != 0)
			{
				while (readDigits() != 0|| readCharacters() != 0)
				{
					read ++;
				}
			}

			if (read != 0 && grabLength() != 0)
			{
				grabTokenType(IoTokenType.HEXNUMBER_TOKEN);
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}


		/// message

		public string nameForGroupChar(char groupChar)
		{
			switch (groupChar)
			{
				case '(': return "";
				case '[': return "squareBrackets";
				case '{': return "curlyBrackets";
			}

			throw new Exception("IoLexer: fatal error - invalid group char" + groupChar);
		}

		public void readMessageError(string name)
		{
			popPosBack();
			errorToken = currentToken();
			errorToken.error = name;
		}

		public int readMessage()
		{
			int foundSymbol = 0;
			pushPos();
			readPadding();
			foundSymbol = readSymbol();

			char groupChar;

			while (readSeparator() != 0 || readComment() != 0);

			groupChar = peekChar();

            // this is bug in original IoVM so I've commented this out

            if ("[{".IndexOf(groupChar) != -1 || (foundSymbol == 0 && 
             groupChar == '('))
			{
				string groupName = nameForGroupChar(groupChar);
				addTokenStringType(groupName, IoTokenType.IDENTIFIER_TOKEN);
			}

			if (readTokenCharsType("([{", IoTokenType.OPENPAREN_TOKEN) != 0)
			{
				readPadding();
				do {
					IoTokenType type = currentToken().type;
					readPadding();
				
					// Empty argument: (... ,)
					if (IoTokenType.COMMA_TOKEN == type)
					{
						char c = current;
						if (',' == c || ")]}".IndexOf(c) != -1)
						{
							readMessageError("missing argument in argument list");
							return 0;
						}
					}

					if (groupChar == '[') specialChars = "._";
					messageChain();
					if (groupChar == '[') specialChars = ":._";
					readPadding();

				} while (readTokenCharType(',', IoTokenType.COMMA_TOKEN) != 0);

				if (readTokenCharsType(")]}", IoTokenType.CLOSEPAREN_TOKEN) == 0)
				{
					if (groupChar == '(')
					{
						readMessageError("unmatched ()s");
					}
					else if (groupChar == '[')
					{
						readMessageError("unmatched []s");
					}
					else if (groupChar == '{')
					{
						readMessageError("unmatched {}s");
					}
					return 0;
				}

				popPos();
				return 1;
			}

			if (foundSymbol != 0)
			{
				popPos();
				return 1;
			}

			popPosBack();
			return 0;
		}

	}


}

