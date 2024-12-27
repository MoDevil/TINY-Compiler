﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tiny_Compiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ClearAll();
            textBox2.Clear();
            dataGridView1.Rows.Clear();
            treeView1.Nodes.Clear();
            string Code=textBox1.Text.ToLower();
            Tiny_Compiler.Start_Compiling(Code);
            PrintTokens();
            treeView1.Nodes.Add(Parser.PrintParseTree(Tiny_Compiler.treeroot));
            PrintErrors();
        }
        void PrintTokens()
        {
            for (int i = 0; i < Tiny_Compiler.Tiny_Scanner.Tokens.Count; i++)
            {
               dataGridView1.Rows.Add(Tiny_Compiler.Tiny_Scanner.Tokens.ElementAt(i).lex, Tiny_Compiler.Tiny_Scanner.Tokens.ElementAt(i).token_type);
            }
        }

        void PrintErrors()
        {
            for(int i=0; i<Errors.Error_List.Count; i++)
            {
                textBox2.Text += Errors.Error_List[i];
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //textBox1.Text = "";
            //textBox2.Text = "";
            //Tiny_Compiler.TokenStream.Clear();
            //dataGridView1.Rows.Clear();
            //treeView1.Nodes.Clear();
            //Errors.Error_List.Clear();

            //// Clear UI elements
            //textBox1.Text = "";
            //textBox2.Text = "";
            //dataGridView1.Rows.Clear();
            //treeView1.Nodes.Clear();

            //// Reset compiler states
            //Tiny_Compiler.TokenStream.Clear();
            //Errors.Error_List.Clear();
            //Tiny_Compiler.treeroot = null;

            ClearAll();

        }
        private void ClearAll()
        {
            // Clear UI elements
            textBox1.Clear();
            textBox2.Clear();
            dataGridView1.Rows.Clear();
            treeView1.Nodes.Clear();

            // Reset compiler state
            Tiny_Compiler.TokenStream.Clear();
            Errors.Error_List.Clear();
            if (Tiny_Compiler.Tiny_Parser != null)
            {
                Tiny_Compiler.Tiny_Parser.Reset();
            }
            Tiny_Compiler.treeroot = null;
        }

    }
}
