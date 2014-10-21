using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calc
{
	class PolaScript
	{

		static string[] token;
		static int ix;
		static Dictionary<string, Constant> varlist = new Dictionary<string, Constant>();
		//3 + 2
		static void Main(string[] args)
		{
			while (true)
			{
				string dat = Console.ReadLine();
				//Console.WriteLine(ArrayToString(GetTokenByExpression(dat)));

				try
				{
					string[] hoge = GetTokenByExpression(dat);

					Console.WriteLine("Tokens: {0}", string.Join(", ", hoge));

					Expression(hoge);
					ConstExpression(hoge);
				}
				catch (Exception ex)
				{
					System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
					Console.Error.WriteLine("[ERROR!!!: {0}]{1}\r\nStackTrace: \r\n", ex.GetType().Name, ex.Message);
					foreach (var frame in trace.GetFrames())
					{
						Console.Error.WriteLine("{0}, {1} : {2}", frame.GetFileLineNumber(), frame.GetFileColumnNumber(), frame.GetMethod());
					}
				}
			}
		}


		

		static string ArrayToString(object[] array)
		{
			string lastdata = "";
			foreach (object val in array)
			{
				lastdata += val.ToString() + " ";
			}
			
			return lastdata;
		}

		static string[] GetTokenByExpression(string source)
		{
			TokenMode tm = TokenMode.Null;
			string tmp = "";
			List<string> lastdata = new List<string>();
			bool isString = false;
			int c = 0;
			foreach (char chr in source)
			{
				if (!isString)
				{
					if (char.IsSeparator(chr))
					{
						if (tmp != "")
						{
							lastdata.Add(tmp);
							tmp = "";
						}
						continue;
					}

					TokenMode nowtm = TokenMode.Null;
					if (char.IsNumber(chr) || chr == '.' || chr == '\\')
						nowtm = TokenMode.Value;
					else if (
							 chr == '+' || chr == '-'
							 || chr == '*' || chr == '/' || chr == '%'
							 || chr == '(' || chr == ')'
							 || chr == '<' || chr == '>' || chr == '='
							 || chr == '&' || chr == '|' || chr == '^'
							 || chr == '!' || chr == '~'
							)
					{
						nowtm = TokenMode.Operation;
					}
					if ((nowtm != tm || (nowtm == TokenMode.Operation && tm == TokenMode.Operation)))
					{
						tm = nowtm;
						if (tmp != "" && tmp + chr != "<=" && tmp + chr != ">=" && tmp + chr != "==" && tmp + chr != "!=" && tmp + chr != "<<" && tmp + chr != ">>"
							&& tmp + chr != "+=" && tmp + chr != "-=" && tmp + chr != "*=" && tmp + chr != "/=" && tmp + chr != "&=" && tmp + chr != "|=" && tmp + chr != "^=")
						{
							lastdata.Add(tmp);
							tmp = "";
						}
					}
				}

				if (c > 0)
				{
					if (chr == '"' && (tmp + chr).Substring(tmp.Length - 2) != "\\\"")
						isString = !isString;
				}
				else
					if (chr == '"')
						isString = !isString;
				tmp += chr;
				
			}
			if (tmp != "")
			{
				lastdata.Add(tmp);
			}
			return lastdata.ToArray();
		}

		public enum TokenMode
		{
			Null, Value, Operation
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>


		static OperatorNode CreateOperatorNode(string name)
		{
			OperatorNode pnode = new OperatorNode();
			pnode.childs = new List<INode>();
			pnode.name = name;
			return pnode;

		}


		static ConstantNode CreateConstantNode(Constant cnst)
		{
			ConstantNode pnode = new ConstantNode();
			pnode.childs = new List<INode>();
			pnode.constant = cnst;
			return pnode;

		}

		static ConstantNode CreateConstantNode(Types type, object val)
		{
			return CreateConstantNode(new Constant(type, val));
		}

		static void AddChild(ref INode pnode, INode pchild)
		{
			//if (pnode.name == "=" || pnode.name == "+=" || pnode.name == "-=" || pnode.name == "*=" || pnode.name == "/=" || pnode.name == "&=" || pnode.name == "|=" || pnode.name == "^=")
			//	pnode.childs.Insert(0, pchild);
			//else
				pnode.childs.Add(pchild);
			
		}


		static void AddTwoChildren(ref INode pnode, INode pchild1, INode pchild2)
		{
			AddChild(ref pnode, pchild1);
			AddChild(ref pnode, pchild2);
		}


		

		static INode SetAssignment()
		{
			INode pleft = SetLogic();
			
			while (ix < token.Length && (token[ix] == "=" || token[ix] == "+=" || token[ix] == "-=" || token[ix] == "*=" || token[ix] == "/=" || token[ix] == "&=" || token[ix] == "|=" || token[ix] == "^="))
			{
				INode pnode = CreateOperatorNode(token[ix++]);
				INode pright = SetAssignment();
				AddTwoChildren(ref pnode, pleft, pright);
				pleft = pnode;
			}
			return pleft;
		}


			
			
	


		//3 + 2 - 5
		//( - ( + 3 2) 5 ) 

		//a = b = c
		//( = a ( = b c) )
		static INode SetLogic()
		{
			INode pleft = SetBit();
			while (ix < token.Length && (token[ix] == "<" || token[ix] == ">" || token[ix] == "==" || token[ix] == "!=" || token[ix] == "<=" || token[ix] == ">="))
			{
				INode pnode = CreateOperatorNode(token[ix++]);
				INode pright = SetShift();
				AddTwoChildren(ref pnode, pleft, pright);
				pleft = pnode;
			}
			return pleft;
		}

		static INode SetBit()
		{
			INode pleft = SetShift();
			while (ix < token.Length && (token[ix] == "&" || token[ix] == "|" || token[ix] == "^"))
			{
				INode pnode = CreateOperatorNode(token[ix++]);
				INode pright = SetLogic();
				AddTwoChildren(ref pnode, pleft, pright);
				pleft = pnode;
			}
			return pleft;
		}

		static INode SetShift()
		{
			INode pleft = SetAddSub();
			while (ix < token.Length && (token[ix] == "<<" || token[ix] == ">>"))
			{
				INode pnode = CreateOperatorNode(token[ix++]);
				INode pright = SetAddSub();
				AddTwoChildren(ref pnode, pleft, pright);
				pleft = pnode;
			}
			return pleft;
		}

		static INode SetAddSub()
		{
			INode pleft = SetMulDiv();
			while(ix < token.Length && (token[ix] == "+" || token[ix] == "-"))
			{
				INode pnode = CreateOperatorNode(token[ix++]);
				INode pright = SetMulDiv();
				AddTwoChildren(ref pnode, pleft, pright);
				pleft = pnode;
			}
			return pleft;
		}

		static INode SetMulDiv()
		{
			INode pleft = SetFactor();
			while (ix < token.Length && (token[ix] == "*" || token[ix] == "/" || token[ix] == "%"))
			{
				INode pnode = CreateOperatorNode(token[ix++]);
				INode pright = SetFactor();
				AddTwoChildren(ref pnode, pleft, pright);
				pleft = pnode;
			}
			return pleft;
		}



		static INode SetFactor()
		{
			INode pnode;
			if (token[ix] == "(")
			{
				ix++;
				pnode = SetAssignment();
				if (token[ix++] != ")")
					throw new Exception("\")\"がありません。");
			}
			else if (token[ix] == "-" || token[ix] == "~" || token[ix] == "!")
			{
				pnode = CreateOperatorNode(token[ix++]);
				INode pright = SetAssignment();
				AddChild(ref pnode, pright);
			}
			else if (token[ix] == "num" || token[ix] == "string" || token[ix] == "bool" || token[ix] == "Number" || token[ix] == "String" || token[ix] == "Boolean")
			{
				INode pleft = null;
				switch (token[ix])
				{
					case "num":
						pleft = CreateConstantNode(Types.Type, Types.Number);
						break;
					case "Number":
						pleft = CreateConstantNode(Types.Type, Types.Number);
						break;
					case "string":
						pleft = CreateConstantNode(Types.Type, Types.String);
						break;
					case "String":
						pleft = CreateConstantNode(Types.Type, Types.String);
						break;
					case "bool":
						pleft = CreateConstantNode(Types.Type, Types.Boolean);
						break;
					case "Boolean":
						pleft = CreateConstantNode(Types.Type, Types.Boolean);
						break;
					default:
						throw new Exception(string.Format("{0} という型指定子はありません。"));

				}
				pnode = CreateOperatorNode("let");
				ix++;
				INode pright = SetFactor();

				AddTwoChildren(ref pnode, pleft, pright);
				pright = pnode;
			}
			else
			{
				string tmp = token[ix++];
				if (isNumber(tmp))	//数値
				{
					pnode = CreateConstantNode(new Constant(Types.Number, double.Parse(tmp)));
				}
				else if (isVarName(tmp))	//変数
				{
					pnode = CreateConstantNode(new Constant(Types.Variable, tmp));
				}
				else if (tmp[0] == '"' && tmp[tmp.Length - 1] == '"' && tmp.Length > 1)	//文字列
				{
					pnode = CreateConstantNode(Types.String, tmp.Substring(1, tmp.Length - 2));
				}
				else if (tmp == "true" || tmp == "false")	//ブール
				{
					pnode = CreateConstantNode(Types.Boolean, bool.Parse(tmp));
				}
				else
				{
					throw new Exception(tmp + " を理解できません。");
				}
			}
			return pnode;
		}

		static bool isNumber(string val)
		{
			double d;
			return double.TryParse(val, out d);
		}

		static bool isVarName(string val)
		{
			bool lastdata = true;
			int cnt = 0;
			foreach (char c in val)
			{
				if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (cnt > 0 && c >= '0' && c <= '9') || (c == '_')))
				{
					lastdata = false;
				}
				cnt++;
			}
			return lastdata;
		}


		static string GetAST(INode node)
		{
			StringBuilder lastdata = new StringBuilder();
			lastdata.AppendFormat(" ({0}", node.name);
			for (int n = 0; n < node.childs.Count; n++)
			{
				INode pchild = node.childs[n];
				if (pchild is ConstantNode)
				{
					lastdata.Append(" ");
					ConstantNode cnode = (ConstantNode)pchild;
					lastdata.Append(cnode.constant.type);
					lastdata.Append(" ");
					switch(cnode.constant.type)
					{
						case Types.Number:
							lastdata.Append((double)cnode.constant.value);
							break;
						case Types.Boolean:
							lastdata.Append((bool)cnode.constant.value);
							break;
						case Types.String:
							lastdata.Append((string)cnode.constant.value);
							break;
						case Types.Variable:
							lastdata.Append((string)cnode.constant.value);
							break;
						case Types.Type:
							lastdata.Append((Types)cnode.constant.value);
							break;
						case Types.Null:
							lastdata.Append("null");
							break;
					}
				}
				else
					lastdata.Append(GetAST(pchild));
			}
			lastdata.Append(")");

			return lastdata.ToString();
		}

		static void Expression(string[] intoken)
		{
			foreach (string data in intoken)
				Console.Write(data + " ");
			Console.Write(" => ");

			token = intoken;
			ix = 0;
			INode expr = SetAssignment();
			Console.Write(GetAST(expr));
			Console.WriteLine();
		}

		static Constant Calc(INode pnode)
		{

			if (pnode is ConstantNode)
				return ((ConstantNode)pnode).constant;


			Constant p1, p2;
			Constant _p1, _p2;
			switch (pnode.childs.Count)
			{
				case 2:
					p1 = Calc(pnode.childs[0]);
					p2 = Calc(pnode.childs[1]);


					
					if (p1.type == Types.Variable)
						_p1 = varlist[(string)p1.value];
					else
						_p1 = p1;
					if (p2.type == Types.Variable && p1.type != Types.Type)
						_p2 = varlist[(string)p2.value];
					else
						_p2 = p2;

					
					if (pnode.name == "+")
					{

						if (_p1.type == Types.String && _p2.type == Types.Number)
							return new Constant(Types.String, (string)_p1.value + ((double)_p2.value).ToString());
						else if (_p1.type == Types.String && _p2.type == Types.String)
							return new Constant(Types.String, (string)_p1.value + (string)_p2.value);
						else if (_p1.type == Types.Number && _p2.type == Types.Number)
							return new Constant(Types.Number, (double)_p1.value + (double)_p2.value);
						else
							throw new Exception(string.Format("{0} 演算子を、 {1} 型と {2} 型の演算に使用できません。", pnode.name, _p1.type, _p2.type)); 
					}
					else if (pnode.name == "-")
						return new Constant(Types.Number, (double)_p1.value - (double)_p2.value);
					else if (pnode.name == "*")
					{
						if (_p1.type == Types.String && _p2.type == Types.Number)
							return new Constant(Types.String, (RepeatString((string)_p1.value, (int)(double)_p2.value)));
						return new Constant(Types.Number, (double)_p1.value * (double)_p2.value);
					}
					else if (pnode.name == "/")
						return new Constant(Types.Number, (double)_p1.value / (double)_p2.value);
					else if (pnode.name == "%")
						return new Constant(Types.Number, (double)_p1.value % (double)_p2.value);
					else if (pnode.name == "<")
						return new Constant(Types.Boolean, ((double)_p1.value < (double)_p2.value) ? true : false);
					else if (pnode.name == ">")
						return new Constant(Types.Boolean, ((double)_p1.value > (double)_p2.value) ? true : false);
					else if (pnode.name == "<=")
						return new Constant(Types.Boolean, ((double)_p1.value <= (double)_p2.value) ? true : false);
					else if (pnode.name == ">=")
						return new Constant(Types.Boolean, ((double)_p1.value >= (double)_p2.value) ? true : false);
					else if (pnode.name == "==")
						return new Constant(Types.Boolean, ((double)_p1.value == (double)_p2.value) ? true : false);
					else if (pnode.name == "!=")
						return new Constant(Types.Boolean, ((double)_p1.value != (double)_p2.value) ? true : false);
					else if (pnode.name == "<<")
						try
						{
							if ((double)_p1.value - (int)_p1.value != 0 || (double)_p2.value - (int)_p2.value != 0)
								throw new Exception(" << 演算は、整数のみ行うことができます。");
							else
								return new Constant(Types.Number, (int)_p1.value << (int)_p2.value);
						}
						catch(InvalidCastException)
						{
							throw new Exception(string.Format("{0} 演算子を、 {1} 型と {2} 型の演算に使用できません。", pnode.name, _p1.type, _p2.type)); 
						}
					else if (pnode.name == ">>")
						if ((double)_p1.value - (int)_p1.value != 0 || (double)_p2.value - (int)_p2.value != 0)
							throw new Exception(" >> 演算は、整数のみ行うことができます。");
						else
							return new Constant(Types.Number, (int)_p1.value >> (int)_p2.value);
					else if (pnode.name == "&")
						if ((double)_p1.value - (int)_p1.value != 0 || (double)_p2.value - (int)_p2.value != 0)
							throw new Exception(" & 演算は、整数のみ行うことができます。");
						else
							return new Constant(Types.Number, (int)_p1.value & (int)_p2.value);
					else if (pnode.name == "|")
						if ((double)_p1.value - (int)_p1.value != 0 || (double)_p2.value - (int)_p2.value != 0)
							throw new Exception(" | 演算は、整数のみ行うことができます。");
						else
							return new Constant(Types.Number, (int)_p1.value | (int)_p2.value);
					else if (pnode.name == "^")
						if ((double)_p1.value - (int)_p1.value != 0 || (double)_p2.value - (int)_p2.value != 0)
							throw new Exception(" ^ 演算は、整数のみ行うことができます。");
						else
							return new Constant(Types.Number, (int)_p1.value ^ (int)_p2.value);
					else if (pnode.name == "=")
					{
						
						
						if (p1.type == Types.Variable)
						{
							CheckVarExists(p1);
							return varlist[(string)p1.value].SetValue(_p2.type, _p2.value);
						}
					}
					else if (pnode.name == "+=")
					{
						//複合代入演算子に、変数が参照できないときの処理を追加する(キーが存在しないとき)
						
						CheckVarExists(p1);
						if (varlist[(string)p1.value].type == Types.String && _p2.type == Types.String)
							return varlist[(string)p1.value].SetValue(_p2.type, (string)(varlist[(string)p1.value].value) + (string)_p2.value);
						else if (varlist[(string)p1.value].type == Types.String && _p2.type == Types.Number)
							return varlist[(string)p1.value].SetValue(varlist[(string)p1.value].type, (string)(varlist[(string)p1.value].value) + (double)_p2.value);
						else if (varlist[(string)p1.value].type == Types.Number && _p2.type == Types.Number)
							return varlist[(string)p1.value].SetValue(_p2.type, (double)(varlist[(string)p1.value].value) + (double)_p2.value);
						else
							throw new Exception(string.Format("{0} 演算子を、 {1} 型と {2} 型の演算に使用できません。", pnode.name, _p1.type, _p2.type));
					}
					else if (pnode.name == "-=")
					{
						CheckVarExists(p1);
						if (varlist[(string)p1.value].type == Types.Number && p2.type == Types.Number)
							return varlist[(string)p1.value].SetValue(p2.type, (double)(varlist[(string)p1.value].value) - (double)p2.value);
						else
							throw new Exception(string.Format("{0} 演算子を、 {1} 型と {2} 型の演算に使用できません。", pnode.name, p1.type, p2.type));
					}
					else if (pnode.name == "*=")
					{
						CheckVarExists(p1);
						if (varlist[(string)p1.value].type == Types.Number && p2.type == Types.Number)
							return varlist[(string)p1.value].SetValue(p2.type, (double)(varlist[(string)p1.value].value) + (double)p2.value);
						else if (varlist[(string)p1.value].type == Types.String && p2.type == Types.Number)
							return varlist[(string)p1.value].SetValue(varlist[(string)p1.value].type, RepeatString((string)(varlist[(string)p1.value].value), (int)p2.value));
						else
							throw new Exception(string.Format("{0} 演算子を、 {1} 型と {2} 型の演算に使用できません。", pnode.name, p1.type, p2.type));
					}
					else if (pnode.name == "/=")
					{
						CheckVarExists(p1);
						if (varlist[(string)p1.value].type == Types.Number && p2.type == Types.Number)
							return varlist[(string)p1.value].SetValue(p2.type, (double)(varlist[(string)p1.value].value) / (double)p2.value);
						else
							throw new Exception(string.Format("{0} 演算子を、 {1} 型と {2} 型の演算に使用できません。", pnode.name, p1.type, p2.type));
					}
					else if (pnode.name == "let")
					{
						if (p1.type != Types.Type)
							throw new Exception("型指定子がありません。");
						if (p2.type != Types.Variable)
							throw new Exception((string)p2.value + " は変数名として不適切です。");
						if (varlist.ContainsKey((string)p2.value))
							throw new Exception("変数 " + (string)p2.value + " はすでに存在します。");
						varlist[(string)p2.value] = new Constant(((Types)p1.value), null);
						return p2;
					}
					break;
				case 1:
					p1 = Calc(pnode.childs[0]);
					if (p1.type == Types.Variable)
						_p1 = varlist[(string)p1.value];
					else
						_p1 = p1;


					if (pnode.name == "-")
						return new Constant(Types.Number, -(double)p1.value);
					else if (pnode.name == "~")
						if ((double)p1.value - (int)p1.value != 0)
							throw new Exception("~ 演算は、整数のみ行うことができます。");
						else
							return new Constant(Types.Number, ~(int)p1.value);
					else if (pnode.name == "!")
					{
						if ((double)p1.value != 0)
							return new Constant(Types.Number, 1);
						else
							return new Constant(Types.Number, 0);
					}
					break;
				default:
					throw new Exception(string.Format("{0}つの項をとる演算子はありません。", pnode.childs.Count));
			}
			throw new Exception(string.Format("{0}つの項をとる、\"{1}\" 演算子はありません。", pnode.childs.Count, pnode.name));
			
		}

		public static string RepeatString(string s, int count)
		{
			System.Text.StringBuilder buf =
				new System.Text.StringBuilder(s.Length * count);
			for (int i = 0; i < count; i++)
			{
				buf.Append(s);
			}
			return buf.ToString();
		}

		static void ConstExpression(string[] intoken)
		{
			token = intoken;
			ix = 0;
			while (ix < token.Length)
			{
				INode expr = SetAssignment();
				Constant cst = Calc(expr);
				Console.WriteLine(cst.type + " " + cst.value);
			}
		}

		
		/// <summary>
		/// 指定した Constant が変数を表すか、表すならその変数が存在するかどうか確認し、問題があれば例外を投げます。
		/// </summary>
		/// <param name="vardata">チェックする Constant 構造体。</param>
		static void CheckVarExists(Constant vardata)
		{
			if (vardata.type != Types.Variable)
				throw new Exception("左辺は変数を表しません。");
			if (!varlist.ContainsKey((string)vardata.value))
				throw new Exception(string.Format("変数 {0} は宣言されていません。", (string)vardata.value));
		}
		
	}



	interface INode
	{
		string name {get; set;}

		List<INode> childs { get; set; }

		
	}

	class OperatorNode : INode
	{
		public string name { get; set; }
		public List<INode> childs { get; set; }
		public string value;
	}

	class ConstantNode : INode
	{
		public string name { get; set; }
		public List<INode> childs { get; set; }
		public Constant constant;
	}

	/*struct Variable
	{
		//public Types type;
		public string name;
		public Constant value;

		public void SetValue(string nm, Constant con)
		{
			name = nm;
			if (this.value.type == con.type || this.value.type == Types.Null)
			{
				this.value.type = con.type;
				this.value = con;
			}
			else
			{

				if (this.value.type == Types.Number && con.type == Types.String)
				{
					con.value = double.Parse((string)con.value);
				}
				else if (this.value.type == Types.String && con.type == Types.Number)
				{
					con.value = ((int)con.value).ToString();
				}
				else
					throw new Exception(string.Format("{0} から {1} への変換はできません。", con.type.ToString(), this.value.type.ToString()));
			}
		}

	}*/

	class Constant
	{
		public Types type { get; set; }
		public object value { get; set; }

		public Constant(Types t, object val)
		{
			type = t;
			SetValue(t, val);
		}

		public Constant SetValue(Types t, object val)
		{
			if (type != t)
				throw new Exception(string.Format("{0} 型と {1} 型で一致しません。", type, t));

			if (val == null)
			{
				value = val;
				return this;
			}
			switch (t)
			{
				case Types.Number:
					if (!(val is double || val is int))
						throw new Exception(string.Format("{0} 型と .NET の {1} 型で一致しません。", t, val.GetType().Name));
					break;
				case Types.Boolean:
					if (!(val is bool))
						throw new Exception(string.Format("{0} 型と .NET の {1} 型で一致しません。", t, val.GetType().Name));
					break;
				case Types.String:
					if (!(val is string))
						throw new Exception(string.Format("{0} 型と .NET の {1} 型で一致しません。", t, val.GetType().Name));
					break;
			}
			value = val;
			return this;
		}

		
	}

	enum Types
	{
		Null, Number, String, Boolean, Variable, Type
	}

}
