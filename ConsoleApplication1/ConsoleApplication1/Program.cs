using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace ConsoleApplication1
{

    class article
    {
        public string i="";
        public string t="";
        public string w="";
    }
    class Program
    {
        static void Main(string[] args)
        {
            //未做事項
            //前處裡與計算log斷詞用regex完善 
            //
            //如果要做bigram 要做雙字詞組合字典c(wn-1,wn) 並且考慮保留<s> , . !?等標點符號以共判斷上下文
            //
            //使用其他平滑



            

            if (File.Exists(@"..\..\num.txt") && File.Exists(@"..\..\cw.txt"))
            {
                Console.WriteLine("偵測到存在字典與字數");
            }
            else
            {
                Console.WriteLine("偵測到不存在字典與字數，開始建立");
                pre();
                Console.WriteLine("字典與字數建立完成");
            }
            
            
            Console.WriteLine("使用unigram");
            while (true)
            {
                Console.WriteLine("請輸入句子(-1離開):");
                string s = Console.ReadLine();


                if (s != "-1")
                {
                    unigram(s);
                }
                else 
                {
                    break;
                }
            }
        }

        static void unigram(string  hh)
        {
            string pattern = @"[^\"" ]+(?=\;\ |\:\ |\,\""|\.\""|\!\""|\?\""|,\ |\.\ |\.$|\.\n)|(?<=\ )[^\"" ]+(?=\ )|(?<=\"")[^\"" ]+(?=\ )|(?<= )[^\"" ]+(?=\"")|^[^\"" ]+(?=\ )|[\.,\?\!\""\:\;]|(?<=\"")[^ \""]+(?=\"")|(?<=\n)[^ \""]+(?=\ )";
            //[^" ]+(?=\;\ |\:\ |\,"|\."|\!"|\?"|,\ |\.\ |\.$|\.\n)
            //
            //|(?<=\ )[^" ]+(?=\ )|(?<=")[^" ]+(?=\ )
            //
            //|(?<= )[^" ]+(?=")|^[^" ]+(?=\ )
            //
            //|[\.,\?\!"\:\;]
            //
            //|(?<=")[^ "]+(?=")
            //
            //|(?<=\n)[^ "]+(?=\ )

            int testwordcount = 0;
            string[] testword=new string[10000];
            foreach (Match m in Regex.Matches(hh, pattern))
            {
                testword[testwordcount] = m.Value;
                testwordcount++;
            
            }


            string[] temp = File.ReadAllLines(@"..\..\num.txt");
            int num =  Int32.Parse(temp[0]);
            var res = File
                .ReadAllLines(@"..\..\cw.txt")
                .Select(l => l.Split('~'))//抓到key value
                .ToDictionary(a => a[0], a =>  Int32.Parse(a[1]));//轉成新字典
            double prolog = 0;
            Console.WriteLine("\n{0,-100}{1,-100}\n\n", "word", "Log10 probability");
            for (int i = 0; i < testwordcount; i++)
            {
                double temp2 = 0;
                int actualValue = 0;
                if (!res.TryGetValue(testword[i], out actualValue))
                {
                    temp2 += Math.Log10((double)(1) / (double)(num + res.Count));
                }
                else
                {
                    temp2 += Math.Log10((double)(res[testword[i]] + 1) / (double)(num + res.Count));
                }
                prolog += temp2;
                Console.WriteLine("{0,-100}{1,-100}", testword[i], temp2);
            }
            Console.WriteLine("\n{0,-100}{1,-100}\n\n", "The sum of all probabilities", prolog);
        }

        static void pre() //前置處理 產生單字出現次數 總字數
        {
            string pattern = @"[^\"" ]+(?=\;\ |\:\ |\,\""|\.\""|\!\""|\?\""|,\ |\.\ |\.$|\.\n)|(?<=\ )[^\"" ]+(?=\ )|(?<=\"")[^\"" ]+(?=\ )|(?<= )[^\"" ]+(?=\"")|^[^\"" ]+(?=\ )|[\.,\?\!\""\:\;]|(?<=\"")[^ \""]+(?=\"")|(?<=\n)[^ \""]+(?=\ )";

            string[] lines = System.IO.File.ReadAllLines(@"..\..\ohsumed.87");
            int status = 0; int cou = -1;
            article[] tt = new article[54710];
            int cou1 = 0, cou2 = 0, cou3 = 0;

            foreach (string line in lines)//先從row data讀出標題和摘要到物件陣列 以方便操作
            {
                if (status == 1)
                {
                    tt[cou] = new article();
                    tt[cou].i = line;
                    status = 0;

                }
                if (status == 2)
                {
                    tt[cou].t = line;
                    status = 0;
                }
                if (status == 3)
                {
                    tt[cou].w = line;
                    status = 0;

                }
                if (line.StartsWith(".I "))
                {
                    status = 1;
                    cou++;
                    cou1++;
                }
                if (line.Equals(".T"))
                {
                    status = 2;
                    cou2++;
                }

                if (line.Equals(".W"))
                {
                    status = 3;
                    cou3++;
                }
            }









            var dict1 = new Dictionary<string, int>();
            var dict2 = new Dictionary<string, int>();
            var dict3 = new Dictionary<string, int>();
            var dict4 = new Dictionary<string, int>();


            //跑多執行序統計物件陣列中的總字數和單一字詞出現字數
            int[] wordcount = new int[] { 0, 0, 0, 0 };
            int MAX_COUNT = 54710;//0-54709

            //    //控制Thread數
            int WORKER_COUNT = 4;//我是四核心故設定4
            Thread[] workers = new Thread[WORKER_COUNT];
            int jobsCountPerWorker = 13678;
            for (int i = 0; i < WORKER_COUNT; i++)
            {
                //將全部工作切成WORKER_COUNT份，
                //分給WORKER_COUNT個Thread執行
                int st = jobsCountPerWorker * i;        //0     ,13678,27356,41034
                int ed = jobsCountPerWorker * (i + 1);  //13677 ,27355,41033,54711(改成54709)
                if (ed > MAX_COUNT) ed = MAX_COUNT;     //54711改成54709

                workers[i] = new Thread(() =>
                {
                    Console.WriteLine("LOOP: {0:N0} - {1:N0}", st, ed);
                    var dicttemp = new Dictionary<string, int>();
                    for (int j = st; j < ed; j++)
                    {
                        if (tt[j].w != "") 
                        {
                            foreach (Match m in Regex.Matches(tt[j].w, pattern))
                            {
                                wordcount[st / 13678] ++;//算總字數
                                if (st / 13678 == 0)
                                {
                                    a1(dict1, m.Value);
                                }
                                else if (st / 13678 == 1)
                                {
                                    a1(dict2, m.Value);
                                }
                                else if (st / 13678 == 2)
                                {
                                    a1(dict3, m.Value);
                                }
                                else if (st / 13678 == 3)
                                {
                                    a1(dict4, m.Value);
                                }
                            } 
                        }
                    }


                   




                });
                workers[i].Start();
            }
            for (int i = 0; i < WORKER_COUNT; i++)
                workers[i].Join();

            var result = dict1.Concat(dict2).Concat(dict3).Concat(dict4)//串聯分工結果
                .GroupBy(d => d.Key)//依照key GroupBy合併value
                .OrderBy(d => d.Key)
                .ToDictionary(d => d.Key, d => d.First().Value);//轉換成新的字典
            //合併字詞出現次數工作成果
            
            foreach (var te in result)
            {
                WriteLog(te.Key + "~" + te.Value);
                //寫字詞出現次數檔案
            }

            StreamWriter outputFile = new StreamWriter(@"..\..\num.txt", true);
            outputFile.WriteLine((wordcount[0] + wordcount[1] + wordcount[2] + wordcount[3]));//寫總字數檔案
            outputFile.Close();
        }

        static void  a1(Dictionary<string, int> dicttemp,string test)//字典單字記數
        {
            int actualValue=0;
            if (!dicttemp.TryGetValue(test, out actualValue))
            {
                dicttemp[test] = 1;
            }
            else
            {
                dicttemp[test]++;
            }
        }

        static async void WriteTextAsync(string text)//非同步方式輸出 會暫存等待 解決資料同步問題
        {
            using (StreamWriter outputFile = new StreamWriter(@"..\..\cw2.txt", true))
            {
                await outputFile.WriteAsync(text+"\n");
            }
        }

        //目前用這個
        static object lockMe = new object();
        static void WriteLog(string text)//開多執行序輸出 用lock確保一次做一個執行序 解決資料同步問題
        {
            lock (lockMe)
            {
                using (StreamWriter outputFile = new StreamWriter(@"..\..\cw.txt", true))
                {
                    outputFile.WriteLine(text);
                    outputFile.Close();
                }
            }
        }
    }
}
