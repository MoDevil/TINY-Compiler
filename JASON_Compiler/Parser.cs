using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
namespace Tiny_Compiler
{
    
    public class Node
    {
        public List<Node> Children = new List<Node>();
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    
    public class Parser
    {
        private int TokenIndex = 0;
        List<Token> TokenStream;
        public Node root;
        //private Boolean MainFunctionFound = false;
       
        public void Reset()
        {
            this.TokenIndex = 0;
            this.TokenStream = null;
            this.root = null;
            //this.MainFunctionFound = false;

        }

        public Node StartParsing(List<Token> TokenStream)
        {
            Reset();
            if (Errors.Error_List.Count != 0 )
            {
                return null;
            }
            if (TokenStream == null || TokenStream.Count == 0)
            {
                // Handle empty input case
                return null; 
            }
            this.TokenStream = TokenStream;
            root = new Node("Root Node");
            root.Children.Add(Program());

            //if (!MainFunctionFound)
            //    Errors.Error_List.Add("Error: Main function not found in the program! \n");

            return root;
        }
        private Node Program()
        {
            Node node = new Node("Program");
            node.Children.Add(Functions());
            node.Children.Add(MainFunction());
            return node;
        }

        private Node Functions()
        {
            int currentindex = TokenIndex;
            Node node = new Node("Functions");
            if (IsDataType())
            {
                
                if (!IsMainFunction()) // Only process if it's not the main function
                {
                    node.Children.Add(FunctionStatement());
                    node.Children.Add(Functions());
                }
                return node;
            }
            else // if the invalid datatype 
            {
                if (TokenIndex >= TokenStream.Count) // in case there is no main function
                {
                    return null ;
                }
                Errors.Error_List.Add($"Syntax Error in line: {TokenStream[TokenIndex].line_number} Error in Function Declaration \n");
                //while (TokenStream[TokenIndex].line_number == TokenStream[currentindex].line_number)
                //{
                //    TokenIndex++;
                //    node.Children.Add(FunctionStatement());
                //    node.Children.Add(Functions());
                //}
                node.Children.Add(FunctionStatement());
                node.Children.Add(Functions());
            }
            return null;
        }
        private Node MainFunction()
        {
            Node node = new Node("Main_Function");
       
            if(TokenIndex < (TokenStream.Count - 2))
            {
                if (TokenStream[TokenIndex].token_type == Token_Class.Int && TokenStream[TokenIndex + 1].token_type == Token_Class.main)
                {
                    node.Children.Add(Match(Token_Class.Int));
                    node.Children.Add(Match(Token_Class.main));
                    node.Children.Add(Match(Token_Class.LParanthesis));
                    node.Children.Add(Match(Token_Class.RParanthesis));
                    node.Children.Add(FunctionBody());
                }
                else
                {
                    Errors.Error_List.Add("Error: Main function not found in the program! \n");
                }
            }
            else
            {
                Errors.Error_List.Add("Error: Main function not found in the program! \n");
            }
            
            return node;
        }
        private Node FunctionStatement()
        {
            Node node = new Node("Function_Statement");
            node.Children.Add(FunctionDeclaration());
            node.Children.Add(FunctionBody());
            return node;
        }
       
        private Node FunctionDeclaration()
        {
            Node node = new Node("Function_Declaration");
            Node datatype = Datatype();
            if (datatype == null)
            {
                Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number} Expected Datatype but Found {TokenStream[TokenIndex].token_type} \n");
                if(TokenIndex < TokenStream.Count && TokenStream[TokenIndex + 1].token_type == Token_Class.Idenifier) // then the incorrect token was supposed to be a datatype but it has typing issue
                {
                    TokenIndex++;
                }
                //then the datatype is missed 
            }
            node.Children.Add(datatype);
            node.Children.Add(FunctionName());
            node.Children.Add(Match(Token_Class.LParanthesis));
            node.Children.Add(Parameters());
            node.Children.Add(Match(Token_Class.RParanthesis));
            return node;
        }
        //private void RecoverInParameters()
        //{
        //    while (TokenIndex < TokenStream.Count)
        //    {
        //        if (ParameterSyncTokens.Contains(TokenStream[TokenIndex].token_type))
        //        {
        //            break;
        //        }
        //        TokenIndex++;
        //    }
        //}
        private Node Parameters()
         {
            int startindex = TokenIndex;
            if (IsDataType() || (TokenStream[TokenIndex + 1].line_number == TokenStream[TokenIndex].line_number && TokenStream[TokenIndex].token_type == Token_Class.Idenifier))// 34an lw el datatybe is missed bs feh params 3ady
            {
                Node node = new Node("Parameters");
                
                Node param = Parameter();
                // there is no param return null
                if (param == null && !(TokenStream[TokenIndex + 1].line_number == TokenStream[startindex].line_number && TokenStream[TokenIndex + 1].token_type != Token_Class.RParanthesis))
                    return null;
                node.Children.Add(param);
                if (IsvalidToken(Token_Class.Comma)) // and the index + 1 != )
                {
                    if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex + 1].token_type == Token_Class.RParanthesis)
                    {
                        Errors.Error_List.Add($"Syntax Error at line Number {TokenStream[TokenIndex].line_number}: There is no parameters after comma \n");
                    }
                    node.Children.Add(Match(Token_Class.Comma));
                    
                    Node moreParams = Parameters();
                    if (moreParams != null)
                        node.Children.Add(moreParams);
                }
                return node;
            }
            return null;
        }
        private Node Parameter()
        {
            Node node = new Node("Parameter");
            Node dataType = Datatype();
            if (dataType == null)
            {   // if there is a  missed datatype and there is id
                if (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == Token_Class.Idenifier)
                {
                    Errors.Error_List.Add($"Syntax Error at line Number {TokenStream[TokenIndex].line_number}: There is missed Datatype \n ");
                }
                else // there is no param at all

                {
                    return null;
                }
            }
            node.Children.Add(dataType);
            
            Node identifier = Match(Token_Class.Idenifier);
            if (identifier == null)
            {
                TokenIndex--; 
                return null;
            }

            node.Children.Add(identifier);
            return node;
        }
        private Node FunctionName()
        {
            return Match(Token_Class.Idenifier);
        }
        private Node FunctionBody()
        {
            Node node = new Node("Function_Body");
            node.Children.Add(Match(Token_Class.LcurlyParanthesis));
            bool isStmtStart = false;
            do
            {
                node.Children.Add(Statements());
                node.Children.Add(ReturnStatement());
                isStmtStart = IsStatementStart()
                || TokenStream[TokenIndex].token_type == Token_Class.Else
                || TokenStream[TokenIndex].token_type == Token_Class.elseif
                || TokenStream[TokenIndex].token_type == Token_Class.end
                || TokenStream[TokenIndex].token_type == Token_Class.until
                ;
                if (TokenStream[TokenIndex].token_type == Token_Class.Else
                || TokenStream[TokenIndex].token_type == Token_Class.elseif
                || TokenStream[TokenIndex].token_type == Token_Class.end
                || TokenStream[TokenIndex].token_type == Token_Class.until)
                {
                    TokenIndex++;
                }
            } while (isStmtStart);

            node.Children.Add(Match(Token_Class.RcurlyParanthesis));
            return node;
        }
       

        private Node Statements()
        {
            if (IsStatementStart() || IsvalidToken(Token_Class.Semicolon)) // brdo 34an el semi-colon case
            {
                Node node = new Node("Statements");
                Node stmt = Statement();
                if (stmt != null)
                    node.Children.Add(stmt);
                node.Children.Add(Statements());
                return node;
            }
            return null;
        }
 
        private Node Statement()
        {
            Node node = new Node("Statement");
            Token currentToken = TokenStream[TokenIndex];

            try
            {
                //if (IsvalidToken(Token_Class.Comment))
                //    node.Children.Add(CommentStatement());
                if (IsDataType())
                    node.Children.Add(DeclarationStatement());
                else if (IsvalidToken(Token_Class.Idenifier))
                {
                    if (TokenStream[TokenIndex + 1].token_type == Token_Class.LParanthesis)
                    {
                        node.Children.Add(FunctionCall());
                        Node semicolon = Match(Token_Class.Semicolon);
                        node.Children.Add(semicolon);
                    }
                    else
                    {
                        node.Children.Add(AssignmentStatement());
                    }
                }
                else if (IsvalidToken(Token_Class.write))
                    node.Children.Add(WriteStatement());
                else if (IsvalidToken(Token_Class.read))
                    node.Children.Add(ReadStatement());
                else if (IsvalidToken(Token_Class.If))
                    node.Children.Add(IfStatement());
                else if (IsvalidToken(Token_Class.repeat))
                    node.Children.Add(RepeatStatement());
                else if (IsvalidToken(Token_Class.Semicolon))
                {
                    TokenIndex++;   
                }
                else
                {
                    // Unrecognized statement
                    Errors.Error_List.Add($"Syntax Error at line {currentToken.line_number}: Unexpected token '{currentToken.token_type}'\n");
                    RecoverFromError();
                    return null;
                }

                if (node.Children.Count > 0 && node.Children[0] != null)
                    return node;
            }
            catch (Exception)
            {
                // Handle any unexpected errors during parsing
                Errors.Error_List.Add($"Error at line {currentToken.line_number}: Invalid statement syntax\n");
                RecoverFromError();
            }

            return null;
        }

        private Node DeclarationStatement()
        {
            Node node = new Node("Declaration_Statement");
            Token startToken = TokenStream[TokenIndex];

            try
            {
                node.Children.Add(Datatype());

                Node declList = DeclarationList();
                if (declList == null)
                {
                    Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Invalid declaration list\n");
                    RecoverFromError();
                    return null;
                }
                node.Children.Add(declList);

                Node semicolon = Match(Token_Class.Semicolon);
                node.Children.Add(semicolon);

                return node;
            }   
            catch (Exception)
            {
                Errors.Error_List.Add($"Error at line {startToken.line_number}: Invalid declaration statement syntax\n");
                RecoverFromError();
                return null;
            }
        }

        private Node DeclarationList()
        {
            Node node = new Node("Declaration_List");

            // DeclItem is required
            Node declItem = DeclItem();
            if (declItem == null) return null;
            node.Children.Add(declItem);

            // Handle DeclarationList_Tail
            node.Children.Add(DeclarationList_Tail());

            return node;
        }
        private Node DeclarationList_Tail()
        {
            Node node = new Node("Declaration_List_Tail");

            if (IsvalidToken(Token_Class.Comma))
            {
                node.Children.Add(Match(Token_Class.Comma));
                node.Children.Add(DeclarationList());
                return node;
            }

            // ε case - empty node represents epsilon production
            return node;
        }
        private Node DeclItem()
        {
            Node node = new Node("Decl_Item");
            node.Children.Add(Match(Token_Class.Idenifier)); 
            node.Children.Add(DeclItemTail()); 
            return node;
        }
        private Node DeclItemTail()
        {
            if (IsvalidToken(Token_Class.assignmentOp))
            {
                Node node = new Node("Decl_Item_Tail");
                node.Children.Add(Match(Token_Class.assignmentOp));
                node.Children.Add(Expression()); 
                return node;
            }
            return null;
        }
        private Node Datatype()
        {
            Node node = new Node("Datatype");
            if (IsvalidToken(Token_Class.Int))
                node.Children.Add(Match(Token_Class.Int));
            else if (IsvalidToken(Token_Class.Float))
                node.Children.Add(Match(Token_Class.Float));
            else if (IsvalidToken(Token_Class.String))
                node.Children.Add(Match(Token_Class.String));
            else
            {
                //error?
                return null;
            }
            return node;
        }

        private Node AssignmentStatement()
        {
            Node node = new Node("Assignment_Statement");

            Node identifier = Match(Token_Class.Idenifier);
            if (identifier == null)
            {
                //SynchronizeToNextStatement();
                RecoverFromError();
                return null;
            }
            node.Children.Add(identifier);
            Node assignOp = Match(Token_Class.assignmentOp);
            if (assignOp == null)
            {
                //SynchronizeToNextStatement();
                RecoverFromError();
                return null;
            }
            node.Children.Add(assignOp);
            Node expr = Expression();
            if (expr == null) 
            {
                //SynchronizeToNextStatement();
                RecoverFromError();
                return null;
            }
            else
            {
                node.Children.Add(expr);
            }
            Node semicolon = Match(Token_Class.Semicolon);
            if (semicolon == null)
            {
                // SynchronizeToNextStatement();
                RecoverFromError();
                //return null;
            }
            node.Children.Add(semicolon);
            return node;
        }
        private Node WriteStatement()
        {
            Node node = new Node("Write_Statement");
            node.Children.Add(Match(Token_Class.write));
            node.Children.Add(WriteTail());
            return node;
        }
        private Node WriteTail()
        {
            Node node = new Node("Write_Tail");
            if (IsvalidToken(Token_Class.endl))
            {
                node.Children.Add(Match(Token_Class.endl));
                node.Children.Add(Match(Token_Class.Semicolon));
            }
            else
            {
                node.Children.Add(Expression());
                node.Children.Add(Match(Token_Class.Semicolon));
            }
            return node;
        }
        private Node ReadStatement()
        {
            Node node = new Node("Read_Statement");
            node.Children.Add(Match(Token_Class.read));
            node.Children.Add(Match(Token_Class.Idenifier)); 
            node.Children.Add(Match(Token_Class.Semicolon));
            return node;
        }


        private Node IfStatement()
        {
            Node node = new Node("If_Statement");
            Token startToken = TokenStream[TokenIndex];
            try
            {
                node.Children.Add(Match(Token_Class.If)); 

                // Parse the condition (which can now handle complex boolean expressions)
                Node condition = ConditionStatement();
                if (condition == null)
                {
                    Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Invalid or missing condition in if statement\n");
                    // Try to recover by looking for 'then'
                    while (TokenIndex < TokenStream.Count &&
                           !IsvalidToken(Token_Class.then) &&
                           !IsvalidToken(Token_Class.end))
                    {
                        TokenIndex++;
                    }
                }
                else
                {
                    node.Children.Add(condition);
                }

                // Look for then keyword
                if (!IsvalidToken(Token_Class.then))
                {
                    Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: Missing 'then' keyword\n");
                    RecoverFromError(strictRecovery: true);
                    return node;
                }
                Node stmt = null;
                node.Children.Add(Match(Token_Class.then));

                // Parse the rest of the if statement
                do {
                    stmt = Statements();
                    node.Children.Add(stmt);
                    while (IsStatementEnd() && !IsvalidToken(Token_Class.end))
                    {
                        int lineNumber = TokenStream[TokenIndex].line_number;
                        Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: UnExpected {TokenStream[TokenIndex].token_type} keyword\n");
                        TokenIndex++;
                        while (TokenStream[TokenIndex].line_number == lineNumber) // to skip the until condition
                        {
                            TokenIndex++;
                        }
                    }
                } while (stmt!= null);
                node.Children.Add(ElseStatement());

                if (!IsvalidToken(Token_Class.end))
                {
                    Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: Missing 'end' keyword for if statement\n");
                    RecoverFromError(strictRecovery: true);
                    return node;
                }
                node.Children.Add(Match(Token_Class.end));

                return node;
            }
            catch (Exception)
            {
                RecoverFromError(strictRecovery: true);
                return node;
            }
        }
        private Node ElseStatement()
        {
            if (IsvalidToken(Token_Class.elseif))
            {
                Node node = new Node("Else_Statement");
                node.Children.Add(Match(Token_Class.elseif));
                node.Children.Add(ConditionStatement());  // adding extra emphasization for the then 
                Node thenode = Match(Token_Class.then);
                if (thenode == null)
                {
                    TokenIndex--;
                }
                node.Children.Add(thenode);
                node.Children.Add(Statements());
                node.Children.Add(ElseStatement());
                return node;
            }
            else if (IsvalidToken(Token_Class.Else))
            {
                Node node = new Node("Else_Statement");
                int elselinenum = TokenStream[TokenIndex].line_number;
                node.Children.Add(Match(Token_Class.Else));
                if (TokenStream[TokenIndex].line_number == elselinenum)
                {
                    Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: Else can't have a condition \n");
                    while (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].line_number == elselinenum)
                    {
                        TokenIndex++;
                    }
                }
                node.Children.Add(Statements());
                return node;
            }
            // for empty epsilon case 
            return null;
        }


        private Node RepeatStatement()
        {
            Node node = new Node("Repeat_Statement");
            Token startToken = TokenStream[TokenIndex];
            

            try
            {
                node.Children.Add(Match(Token_Class.repeat));
                Node stmt = null;
                do
                {
                    stmt = Statements();
                    node.Children.Add(stmt); // if end
                    while (IsStatementEnd() && !IsvalidToken(Token_Class.until))
                    {
                        int lineNumber = TokenStream[TokenIndex].line_number;
                        Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: UnExpected {TokenStream[TokenIndex].token_type} keyword\n");
                        TokenIndex++;
                        while (TokenStream[TokenIndex].line_number == lineNumber)
                        {
                            TokenIndex++;
                        }
                    }
                } while (stmt != null);
                if (!IsvalidToken(Token_Class.until))   // if extra ; what to do?
                {
                    Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: Missing 'until' keyword\n");
                    RecoverFromError(strictRecovery: true);
                    return node;
                }

                node.Children.Add(Match(Token_Class.until));

                Node condition = ConditionStatement();
                if (condition == null)
                {
                    Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: Missing condition after 'until'\n");
                    RecoverFromError(strictRecovery: true);
                }
                else
                {
                    node.Children.Add(condition);
                }

                return node;
            }
            catch (Exception)
            {
                RecoverFromError(strictRecovery: true);
                return node;
            }
        }
        private void RecoverFromError(bool strictRecovery = false)
        {
            while (TokenIndex < TokenStream.Count)
            {
                if (IsvalidToken(Token_Class.Semicolon))
                {
                    TokenIndex++; // Skip the semicolon 34an mtbwz4 elle b3do
                    break;
                }
                // For strict recovery, check additional boundaries lb3d el cases zy el if 
                else if (strictRecovery && (
                    IsvalidToken(Token_Class.end) ||
                    IsvalidToken(Token_Class.RcurlyParanthesis) ||
                    IsvalidToken(Token_Class.Return)))
                {
                    break;
                }
                else if (IsStatementStart() && TokenIndex > 0)
                {
                    break;
                }
                TokenIndex++;
            }
        }
        private Node FunctionCall()
        {
            Node node = new Node("Function_Call");
            node.Children.Add(Match(Token_Class.Idenifier));
            node.Children.Add(Match(Token_Class.LParanthesis));
            node.Children.Add(ArgumentList());
            node.Children.Add(Match(Token_Class.RParanthesis));
            return node;
        }
        private Node ArgumentList()
        {
            if (IsExpressionStart())
            {
                Node node = new Node("Argument_List");
                node.Children.Add(ExpressionList()); 
                return node;
            }
            return null;
        }
        

        private Node ExpressionList()
        {
            Node node = new Node("Expression_List");

            // Expression is required
            Node expression = Expression();  
            if (expression == null) return null;
            node.Children.Add(expression);

            
            node.Children.Add(ExpressionList_Tail());

            return node;
        }

        private Node ExpressionList_Tail()
        {
            Node node = new Node("Expression_List_Tail");

            if (IsvalidToken(Token_Class.Comma))
            {
                node.Children.Add(Match(Token_Class.Comma));
                node.Children.Add(ExpressionList());
                return node;
            }

            // ε case - empty node represents epsilon production
            return node;
        }

        private Node Expression()
        {
            Node node = new Node("Expression");

            if (IsvalidToken(Token_Class.stringInput))
            {
                node.Children.Add(Match(Token_Class.stringInput));
            }
            else if (IsEquationStart())
            {
                Node equation = Equation();
                if (equation == null)
                {
                    // Skip until semicolon or next statement
                    RecoverFromError();
                }
                else
                {
                    node.Children.Add(equation);
                }
            }
            else
            {
                Node term = Term();
                if (term != null)
                    node.Children.Add(term);
            }

            return node;
        }
        
        private Node Equation()
        {
            Node node = new Node("Equation");

            // Term is required
            Node term = Term();
            if (term == null)
            {
                // Error recovery: Skip until we find an operator or end of expression
                while (TokenIndex < TokenStream.Count &&
                       !IsAddOp() &&
                       !IsvalidToken(Token_Class.Semicolon))
                {
                    TokenIndex++;
                }
                return null;
            }

            node.Children.Add(term);
            node.Children.Add(Equation_Tail());

            return node;
        }
        private Node Equation_Tail()
        {
            Node node = new Node("Equation_Tail");

            if (IsAddOp())
            {
             
                Node addOp = AddOp();
                node.Children.Add(addOp);
                Node Eq = Equation();
                if (Eq == null)
                {
                    // Report specific error when no valid term follows an operator
                    Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: Incomplete condition after operator \n");
                    return null;
                }
                node.Children.Add(Eq);
                return node;
            }   

            // ε case - empty node represents epsilon production
            return node;
        }

        private Node Term()
        {
            Node node = new Node("Term");
            
            // Factor is required
            Node factor = Factor();
            if (factor == null)
            {
                // Error recovery Skip until we find an operator or end of term     
                while (TokenIndex < TokenStream.Count &&
                       !IsMultOp() &&
                       !IsAddOp() &&
                       !IsvalidToken(Token_Class.Semicolon))
                {
                    TokenIndex++;
                }
                return null;
            }

            node.Children.Add(factor);
            node.Children.Add(Term_Tail());

            return node;
        }
        private Node Term_Tail()
        {
            Node node = new Node("Term_Tail");

            if (IsMultOp())
            {
                Node multOp = MultOp();
                node.Children.Add(multOp);

                // store 34an lw feh error 
                Token currentToken = TokenStream[TokenIndex];

                Node term = Term();
                if (term == null)
                {
                    // Report specific error when no valid factor follows an operator
                    Errors.Error_List.Add($"Syntax Error at line {currentToken.line_number}: Incomplete expression after operator");
                    return null;
                }
                node.Children.Add(term);
                return node;
            }

            // ε case - empty node represents epsilon production
            return node;
        }

        private Node Factor()
        {
            Node node = new Node("Factor");
            if (IsvalidToken(Token_Class.Number))
            {
                node.Children.Add(Match(Token_Class.Number));
            }
            else if (IsvalidToken(Token_Class.Idenifier))
            {
                if (IsFunctionCall())  // to not adding the identifier twice if function call
                {
                    node.Children.Add(FunctionCall());
                }
                else
                {
                    node.Children.Add(Match(Token_Class.Idenifier));
                }
            }
            else if (IsvalidToken(Token_Class.LParanthesis))
            {
                node.Children.Add(Match(Token_Class.LParanthesis));
                node.Children.Add(Equation());
                Node rparanthesis = Match(Token_Class.RParanthesis); 
                node.Children.Add(rparanthesis);
            }
            else
            {
                return null; 
            }
            return node;
        }

        private bool IsFunctionCall()
        {
            return TokenIndex + 1 < TokenStream.Count &&
                   TokenStream[TokenIndex].token_type == Token_Class.Idenifier &&
                   TokenStream[TokenIndex + 1].token_type == Token_Class.LParanthesis;
        }
 
        private Node ConditionStatement()
        {
            Node node = new Node("Condition_Statement");
            Token startToken = TokenStream[TokenIndex];

            try
            {
                // Parse first condition
                Node firstCondition = Condition();
                if (firstCondition == null)
                {
                    if (TokenIndex >= TokenStream.Count || (!IsExpressionStart() && !IsvalidToken(Token_Class.Semicolon)))
                    {
                        Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Incomplete or invalid condition");
                    }
                    return null;

                }
                node.Children.Add(firstCondition);

                // Handle any additional boolean clauses
                Node booleanClause = BooleanClause();
                if (booleanClause != null)
                {
                    node.Children.Add(booleanClause);
                }

                return node;
            }
            catch (Exception)
            {
                Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Invalid condition statement\n");
                return null;
            }
        }

        private Node BooleanClause()
        {
            if (!IsBooleanOperator())
            {
                // in case of  epsilon 
                return null;
            }

            Node node = new Node("Boolean_Clause");
            Token startToken = TokenStream[TokenIndex];

            try
            {
                node.Children.Add(BooleanOperator());

                // Parse the next condition
                Node nextCondition = Condition(); //ig Condition_Statement  not just condition but errors better rn
                if (nextCondition == null)
                {
                    Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Missing condition after boolean operator\n");
                    return null;
                }
                node.Children.Add(nextCondition);

                // Recursively handle additional boolean clauses
                Node additionalClauses = BooleanClause();  //  divided the Condition_Statement
                if (additionalClauses != null)
                {
                    node.Children.Add(additionalClauses);
                }

                return node;
            }
            catch (Exception)
            {
                Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Invalid boolean clause\n");
                return null;
            }
        }


        private Node Condition()
        {
            if (!IsExpressionStart())
            {
                return null;
            }

            Node node = new Node("Condition");
            Token startToken = TokenStream[TokenIndex];

            try
            {
                Node firstExpr = Expression();
                if (firstExpr == null)  
                {
                    return null;
                }
                node.Children.Add(firstExpr);

                if (!IsvalidToken(Token_Class.LessThanOp) &&
                    !IsvalidToken(Token_Class.GreaterThanOp) &&
                    !IsvalidToken(Token_Class.EqualOp) &&
                    !IsvalidToken(Token_Class.NotEqualOp))
                {
                    Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Missing comparison operator\n");
                    return null;
                }
                Node relOp = RelOp();
                if (relOp == null)
                {
                    return null;
                }
                node.Children.Add(relOp);
                // Check if there's a valid expression start for the right side
                if (!IsExpressionStart() && TokenIndex < TokenStream.Count)
                {
                    Errors.Error_List.Add($"Syntax Error at line {TokenStream[TokenIndex].line_number}: Incomplete condition after operator");
                    return null;
                }

                Node secondExpr = Expression();
                if (secondExpr == null)
                {
                    Errors.Error_List.Add($"Syntax Error at line {startToken.line_number}: Incomplete condition\n");
                    return null;
                }
                node.Children.Add(secondExpr);

                return node;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private Node AddOp()
        {
            Node node = new Node("Add_Op");
            if (IsvalidToken(Token_Class.PlusOp))
                node.Children.Add(Match(Token_Class.PlusOp));
            else
                node.Children.Add(Match(Token_Class.MinusOp));
            return node;
        }

        private Node MultOp()
        {
            Node node = new Node("Mult_Op");
            if (IsvalidToken(Token_Class.MultiplyOp))
                node.Children.Add(Match(Token_Class.MultiplyOp));
            else
                node.Children.Add(Match(Token_Class.DivideOp));
            return node;
        }

        private Node RelOp()
        {
            Node node = new Node("Rel_Op");
            if (IsvalidToken(Token_Class.LessThanOp))
                node.Children.Add(Match(Token_Class.LessThanOp));
            else if (IsvalidToken(Token_Class.GreaterThanOp))
                node.Children.Add(Match(Token_Class.GreaterThanOp));
            else if (IsvalidToken(Token_Class.EqualOp))
                node.Children.Add(Match(Token_Class.EqualOp));
            else
                node.Children.Add(Match(Token_Class.NotEqualOp));
            return node;
        }
        private Node BooleanOperator()
        {
            Node node = new Node("Boolean_Operator");
            if (IsvalidToken(Token_Class.andOp))
                node.Children.Add(Match(Token_Class.andOp));
            else
                node.Children.Add(Match(Token_Class.orOp));
            return node;
        }


        private bool IsBooleanOperator()
        {
            return IsvalidToken(Token_Class.andOp) || IsvalidToken(Token_Class.orOp);
        }
        private bool IsDataType()
        {
            return IsvalidToken(Token_Class.Int) ||
                   IsvalidToken(Token_Class.Float) ||
                   IsvalidToken(Token_Class.String);
        }

        private bool IsMainFunction()
        {
            return TokenIndex + 1 < TokenStream.Count &&
                   TokenStream[TokenIndex].token_type == Token_Class.Int &&
                   TokenStream[TokenIndex + 1].token_type == Token_Class.main;
        }

        private bool IsStatementStart()
        {
            return /*IsvalidToken(Token_Class.Comment) ||*/
                   IsDataType() ||
                   IsvalidToken(Token_Class.Idenifier) ||
                   IsvalidToken(Token_Class.write) ||
                   IsvalidToken(Token_Class.read) ||
                   IsvalidToken(Token_Class.If) ||
                   IsvalidToken(Token_Class.repeat);
        }
        private bool IsStatementEnd()
        {
            return 
                   IsvalidToken(Token_Class.end) ||       
                   IsvalidToken(Token_Class.until) ||   
                   IsvalidToken(Token_Class.RcurlyParanthesis); 
        }


        private bool IsEquationStart()
        {
            return IsvalidToken(Token_Class.Number) ||
                   IsvalidToken(Token_Class.Idenifier) ||
                   IsvalidToken(Token_Class.LParanthesis);
        }

        private bool IsExpressionStart()
        {
            return IsvalidToken(Token_Class.stringInput) ||
                   IsEquationStart();
        }

        private bool IsAddOp()
        {
            return IsvalidToken(Token_Class.PlusOp) ||
                   IsvalidToken(Token_Class.MinusOp);
        }

        private bool IsMultOp()
        {
            return IsvalidToken(Token_Class.MultiplyOp) ||
                   IsvalidToken(Token_Class.DivideOp);
        }
        private Node ReturnStatement()
        {
            Node node = new Node("Return_Statement");
            if (IsvalidToken(Token_Class.Return))
            {
                node.Children.Add(Match(Token_Class.Return));
                Node exp = Expression();
                if (exp == null) {
                    Errors.Error_List.Add($"Syntax Error: at line:{TokenStream[TokenIndex].line_number} invalid Expression after Return Statement \n");
                }
                node.Children.Add(exp);
                
                node.Children.Add(Match(Token_Class.Semicolon));
                return node;
            }
            else 
            {

                Errors.Error_List.Add(FormatError(Token_Class.Return.ToString()));
                return null;
            }
            
        }

        private bool IsvalidToken(Token_Class token)
        {
            return (TokenIndex < TokenStream.Count && TokenStream[TokenIndex].token_type == token);
        }
        private string FormatError(string expectedToken, Token foundToken)
        {
            return $"Syntax Error at line {foundToken.line_number}: Expected '{expectedToken}' but found '{foundToken.token_type}'\n";
        }
        private string FormatError(string expectedToken)
        {
            // For end of file errors
            return $"Syntax Error: Unexpected end of file. Expected '{expectedToken}'\n";
        }
        public Node Match(Token_Class ExpectedToken)
        {

            if (TokenIndex < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[TokenIndex].token_type)
                {
                    TokenIndex++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {

                    Errors.Error_List.Add(FormatError(
                    ExpectedToken.ToString(),
                    TokenStream[TokenIndex]
                    )); 
                    if (ExpectedToken.ToString() == Token_Class.Semicolon.ToString() || ExpectedToken.ToString() == Token_Class.RcurlyParanthesis.ToString() || ExpectedToken.ToString() == Token_Class.LcurlyParanthesis.ToString() || ExpectedToken.ToString() == Token_Class.LParanthesis.ToString() || ExpectedToken.ToString() == Token_Class.RParanthesis.ToString())
                    {
                        TokenIndex--;
                        //RecoverFromError();
                        //return null;
                    }
                    TokenIndex++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add(FormatError(ExpectedToken.ToString()));
                TokenIndex++;
                return null;
            }
        }



        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        public static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
