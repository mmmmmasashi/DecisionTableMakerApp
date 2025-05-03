using DecisionTableLib.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Excel
{
    internal class ExcelRange
    {
        private readonly string? _headerLine;
        private string _tsvText;

        public ExcelRange(string rangeTsvText, bool includeHeaderLine)
        {
            var reader = new StringReader(rangeTsvText);
            if (includeHeaderLine)
            {
                _headerLine = reader.ReadLine();
                _tsvText = reader.ReadToEnd();
            }
            else
            {
                _headerLine = null;
                _tsvText = reader.ReadToEnd();
            }

            SampleCode(rangeTsvText);
        }

        private void SampleCode(string rangeTsvText)
        {
            var lines = rangeTsvText.Split('\n');
            var table = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd('\r').Split('\t').ToList())
                .ToList();
            Trace.WriteLine(lines);
        }

        internal TreeNode ToTree()
        {
            //入力例
            //string sampleText = "因子\t水準\r\nOS\tWindows\r\n\tMac\r\n\tLinux\r\nLanguage\tJapanese\r\n\tEnglish\r\n\tChinese";

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

            // ルートノードを作成
            var root = new TreeNode("root");

            // TSVテキストを行ごとに分割
            var reader = new StringReader(_tsvText);
            var lines = new List<string>();

            while (reader.Peek() >= 0)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }

            // 現在の親ノードを追跡するためのスタック
            var parentStack = new Stack<TreeNode>();
            parentStack.Push(root);

            foreach (var line in lines)
            {
                // 行をタブで分割して階層を判定
                var parts = line.Split('\t');
                var depth = parts.TakeWhile(string.IsNullOrEmpty).Count(); // 空文字の数が深さ

                // 現在のノードを作成
                var nodeName = parts[depth];
                var currentNode = new TreeNode(nodeName);

                // スタックを調整して親ノードを取得
                while (parentStack.Count > depth + 1)
                {
                    parentStack.Pop();
                }

                // 親ノードに現在のノードを追加
                parentStack.Peek().Children.Add(currentNode);

                // 現在のノードをスタックに追加
                parentStack.Push(currentNode);
            }

            return root;


        }
    }
}
