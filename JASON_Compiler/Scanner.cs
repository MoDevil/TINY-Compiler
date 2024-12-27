using System.Collections.Generic;
using System.Text.RegularExpressions;
// condition check //
//<> not equal
//<= less than or equal
//>= grater than or equal
//:= assignment operator
//"words or anything " stringInput
// comment statement /**/
// identifier and reserved keywords
//operators



public enum Token_Class
{
    Semicolon, Comma, LParanthesis, RParanthesis, LcurlyParanthesis, RcurlyParanthesis
        , EqualOp, LessThanOp, stringInput,
    GreaterThanOp, NotEqualOp, PlusOp, MinusOp, MultiplyOp, DivideOp, andOp, orOp, assignmentOp,
    Idenifier, Int, Float, String, read, write, repeat, until, If, elseif, Else, then, Return, endl, end, main,
    Number
}
namespace Tiny_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
        public int line_number; // New property to store line number
    }   

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.read);
            ReservedWords.Add("write", Token_Class.write);
            ReservedWords.Add("until", Token_Class.until);
            ReservedWords.Add("elseif", Token_Class.elseif);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.endl);
            ReservedWords.Add("end", Token_Class.end);
            ReservedWords.Add("main", Token_Class.main);
            ReservedWords.Add("repeat", Token_Class.repeat);
            //ReservedWords.Add("/*", Token_Class.Comment);
            //ReservedWords.Add("*/", Token_Class.Comment);



            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LcurlyParanthesis);
            Operators.Add("}", Token_Class.RcurlyParanthesis);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add(":=", Token_Class.assignmentOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("&&", Token_Class.andOp);
            Operators.Add("||", Token_Class.orOp);

        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();
            int currentLineNumber = 1;
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();
                if (CurrentChar == '\n')
                {
                    currentLineNumber++;
                }

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\t' || CurrentChar == '\n')
                    continue;

                if (CurrentChar >= 'A' && CurrentChar <= 'z')
                {
                    while (j + 1 < SourceCode.Length && ((SourceCode[j + 1] >= 'A' && SourceCode[j + 1] <= 'z') || (SourceCode[j + 1] >= '0' && SourceCode[j + 1] <= '9')))
                    {

                        j++;
                        CurrentLexeme += SourceCode[j].ToString();

                    }

                    FindTokenClass(CurrentLexeme, currentLineNumber);
                    i = j;
                }
                else if (CurrentChar >= '0' && CurrentChar <= '9')

                {

                    while (j + 1 < SourceCode.Length && (SourceCode[j + 1] >= '0' && SourceCode[j + 1] <= '9' || SourceCode[j + 1] == '.'))
                    {

                        j++;
                        CurrentLexeme += SourceCode[j].ToString();
                    }
                    FindTokenClass(CurrentLexeme, currentLineNumber);
                    i = j;
                }
                else if (CurrentChar == '/' && j + 1 < SourceCode.Length && SourceCode[j + 1] == '*')
                {
                    CurrentLexeme = "/*";
                    j += 2;
                    int commentStart = 1;

                    while (j < SourceCode.Length && commentStart > 0)
                    {
                        if (j + 1 < SourceCode.Length && SourceCode[j] == '/' && SourceCode[j + 1] == '*')
                        {
                            commentStart++;
                            CurrentLexeme += "/*";
                            j += 2;
                        }

                        else if (j + 1 < SourceCode.Length && SourceCode[j] == '*' && SourceCode[j + 1] == '/')
                        {
                            commentStart--;
                            CurrentLexeme += "*/";
                            j += 2;
                        }
                        else
                        {
                            CurrentLexeme += SourceCode[j];
                            j++;
                        }
                    }

                    if (commentStart > 0)
                    {
                        Errors.Error_List.Add($"Unclosed comment block on line {currentLineNumber}: {CurrentLexeme}");
                    }

                    i = j - 1;
                }

                else if (CurrentChar == '\"')
                {
                    j++;
                    while (j < SourceCode.Length && SourceCode[j] != '\"')
                    {
                        CurrentLexeme += SourceCode[j].ToString();
                        j++;
                    }
                    if (j < SourceCode.Length)
                    {
                        CurrentLexeme += SourceCode[j].ToString();
                        FindTokenClass(CurrentLexeme, currentLineNumber);
                    }
                    else
                    {
                        Errors.Error_List.Add($"Unclosed string literal on line {currentLineNumber}.");
                    }
                    i = j;
                }
                else
                {
                    if (i + 1 < SourceCode.Length && Operators.ContainsKey(CurrentLexeme + SourceCode[i + 1]))
                    {
                        i++;
                        CurrentLexeme += SourceCode[i].ToString();
                    }
                    FindTokenClass(CurrentLexeme, currentLineNumber);
                }

            }
            Tiny_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex, int lineNumber)
        {
            Token Tok = new Token();
            //Lex = Lex.Trim();
            Tok.lex = Lex;
            Tok.line_number = lineNumber; // Set the line number for the token
            //Is it a reserved word?
            if (ReservedWords.ContainsKey(Lex))
            {

                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
            }
            //Is it an identifier?
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Idenifier;
                Tokens.Add(Tok);
            }
            //Is it a Number?
            else if (isNumber(Lex))
            {
                Tok.token_type = Token_Class.Number;
                Tokens.Add(Tok);
            }
            //Is it an operator?
            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            //Is it string?
            else if (isString(Lex))
            {
                Tok.token_type = Token_Class.stringInput;
                Tokens.Add(Tok);
            }
            //Is it an undefined?
            else
            {
                Errors.Error_List.Add($"Unrecognized lexeme '{Lex}' on line {lineNumber}");

            }
        }


        bool isIdentifier(string lex)
        {
            Regex rx = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);
            bool isValid = false;

            // Check if the lex is an identifier or not.
            if (rx.IsMatch(lex))
            {
                isValid = true;
            }
            return isValid;
        }
        bool isNumber(string lex)
        {
            Regex rx1 = new Regex(@"^[0-9]+(\.[0-9]+)?$", RegexOptions.Compiled);
            bool isValid = false;
            // Check if the lex is  (Number) or not.
            if (rx1.IsMatch(lex))
                isValid = true;
            return isValid;
        }
        bool isString(string lex)
        {
            try
            {
                Regex rx2 = new Regex(@"^""(\\.|[^""\\])*""$", RegexOptions.Compiled);
                return rx2.IsMatch(lex);
            }
            catch
            {
                return false;
            }
        }

    }
}
