﻿using DecisionTableLib.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DecisionTableLib.Excel
{
    public class ExcelRange
    {
        private readonly string _rangeTsvText;

        public ExcelRange(string rangeTsvText)
        {
            _rangeTsvText = rangeTsvText;
        }

        public static (bool IsOk, string ErrorMsg) CheckIfExcelCopiedText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return (false, "クリップボードにテキストがありません。Excelのセルを範囲指定してコピーしてからクリックしてください。");
            }

            if (!(text.Contains("\t") && text.Contains("\r\n")))
            {
                return (false, "クリップボードのテキストが正しい形式ではありません。Excelのセルを範囲指定してコピーしてからクリックしてください。");
            }
            return (true, string.Empty);
        }

        /// <summary>
        /// 列単位で見て、空白のフィールドは上の行の値を引き継ぐ
        /// </summary>
        /// <param name="columns">列の集合体</param>
        private List<List<string>> FullFillTable(List<List<string>> columns)
        {
            var filledColumns = new List<List<string>>(columns.Count);
            for (int i = 0; i < columns.Count; i++)
            {
                filledColumns.Add(new List<string>(columns[i].Count));
                for (int j = 0; j < columns[i].Count; j++)
                {
                    if (string.IsNullOrEmpty(columns[i][j]))
                    {
                        int jSafe = j - 1;
                        var val = (jSafe < 0)? string.Empty : filledColumns[i][jSafe];
                        filledColumns[i].Add(val);
                    }
                    else
                    {
                        filledColumns[i].Add(columns[i][j]);
                    }
                }
            }
            return filledColumns;
        }

        /// <summary>
        /// 列と行を入れ替える
        /// </summary>
        private List<List<string>> ConvertColumnAndRow(List<List<string>> rawTable)
        {
            var columnCount = rawTable.Max(row => row.Count);
            var rowCount = rawTable.Count;
            var convertedTable = new List<List<string>>(columnCount);
            for (int i = 0; i < columnCount; i++)
            {
                convertedTable.Add(new List<string>(rowCount));
                for (int j = 0; j < rowCount; j++)
                {
                    if (i < rawTable[j].Count)
                    {
                        convertedTable[i].Add(rawTable[j][i]);
                    }
                    else
                    {
                        convertedTable[i].Add(string.Empty);
                    }
                }
            }
            return convertedTable;
        }

        /// <summary>
        /// TSV形式のテキストを行列に変換する
        /// </summary>
        private List<List<string>> ToTable(string rangeTsvText)
        {
            var lines = rangeTsvText.Split('\n');
            var table = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))//空行は無視
                .Select(line => line.TrimEnd('\r').Split('\t').ToList())//タブで区切ったリストにする
                .ToList();

            return table;
        }

        public TreeNode ToTree()
        {
            //入力例
            //string sampleText = "OS\tWindows\r\n\tMac\r\n\tLinux\r\nLanguage\tJapanese\r\n\tEnglish\r\n\tChinese";

            //以下のような階層構造として認識したい
            //root
            // L OS
            //   L Windows  
            //   L Mac
            //   L Linux
            // L Language
            //   L Japanese
            //   L English
            //   L Chinese

            var rawRows = ToTable(_rangeTsvText);

            if (rawRows.Count == 0) return new TreeNode("root");

            var rawColumns = ConvertColumnAndRow(rawRows);
            List<List<string>> columns = FullFillTable(rawColumns);

            // ルートノードを作成
            var root = new TreeNode("root");

            //最上位の列が子要素
            var nodes_Layer1 = columns[0]
                .Distinct()
                .Where(name => string.IsNullOrEmpty(name) == false)
                .Select(name => new TreeNode(name));
            root.Children.AddRange(nodes_Layer1);

            //左隣の名前と同じのが親ノード
            var column1 = columns[0];
            var column2 = columns[1];

            for(int i = 0; i < column1.Count; i++)
            {
                var parentName = column1[i];
                var childName = column2[i];

                if (string.IsNullOrEmpty(parentName)) throw new ArgumentException("親ノードの名前が空です");
                //親ノードを探す
                var parentNode = root.Children.FirstOrDefault(node => node.Name == parentName);

                //子ノードを作って親ノードに追加
                if (string.IsNullOrEmpty(childName)) throw new InvalidDataException();
                var childNode = new TreeNode(childName);
                parentNode.Children.Add(childNode);
            }

            return root;
        }

        public List<(string, string)> ToTwoColumnRows()
        {
            var rawRows = ToTable(_rangeTsvText);

            //観点,計算式なので2列しかないはず。それ以外は例外
            if (rawRows.Any(rawRow => rawRow.Count != 2))
            {
                throw new ArgumentException("観点, 計算式の2列のセルをコピーしてから実行してください");
            }

            return rawRows.Select(row => (row[0], row[1])).ToList();
        }
    }
}
