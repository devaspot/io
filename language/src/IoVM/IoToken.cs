
using System;

namespace io {

	public enum IoTokenType {
		NO_TOKEN = 0,
		OPENPAREN_TOKEN,
		COMMA_TOKEN,
		CLOSEPAREN_TOKEN,
		MONOQUOTE_TOKEN,
		TRIQUOTE_TOKEN,
		IDENTIFIER_TOKEN,
		TERMINATOR_TOKEN,
		COMMENT_TOKEN,
		NUMBER_TOKEN,
		HEXNUMBER_TOKEN
	}
	
	public class IoToken {
		string name_;
		IoTokenType type_;
		int charNumber_;
		int lineNumber_;
		IoToken nextToken_;
		string error_;

		public string name { set { name_ = value; } get { return name_; } }
		public IoTokenType type { set { type_ = value; } get { return type_; } }
		public int charNumber { set { charNumber_ = value; } get { return charNumber_; } }
		public int lineNumber  { set { lineNumber_ = value; } get { return lineNumber_; } }
		public IoToken nextToken { 
			set { 
 				if (this == value) 
					throw new Exception("next = self!");
 				nextToken_ = value;
			} 
			get { return nextToken_; }
        }
		public string error { get { return error_; } set { error_ = value; } } 
		public IoToken() { name = null; charNumber = -1; }
		public string typeName() {
			switch (this.type) {
				case IoTokenType.NO_TOKEN:			return "NoToken";
				case IoTokenType.OPENPAREN_TOKEN:	return "OpenParen";
				case IoTokenType.COMMA_TOKEN:		return "Comma";
				case IoTokenType.CLOSEPAREN_TOKEN:	return "CloseParen";
				case IoTokenType.MONOQUOTE_TOKEN:	return "MonoQuote";
				case IoTokenType.TRIQUOTE_TOKEN:	return "TriQuote";
				case IoTokenType.IDENTIFIER_TOKEN:	return "Identifier";
				case IoTokenType.TERMINATOR_TOKEN:	return "Terminator";
				case IoTokenType.COMMENT_TOKEN:		return "Comment";
				case IoTokenType.NUMBER_TOKEN:		return "Number";
				case IoTokenType.HEXNUMBER_TOKEN:	return "HexNumber";
        	}
        	return null;
        }   
        public void quoteName(string name) { name = "\"" + name_ + "\""; }
        public int nameIs(string n) {
			return name.CompareTo(n);  
		}
        public bool isValidMessageName()
        {
            switch (this.type)
            {
                case IoTokenType.IDENTIFIER_TOKEN:
                case IoTokenType.MONOQUOTE_TOKEN:
                case IoTokenType.TRIQUOTE_TOKEN:
                case IoTokenType.NUMBER_TOKEN:
                case IoTokenType.HEXNUMBER_TOKEN:
                    return true;
                default:
                    return false;
            }
        }
   		public void print() { printSelf(); }
 		public void printSelf() { System.Console.Write("'" + name + "'"); }

	}
}
