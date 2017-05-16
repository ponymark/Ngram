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
        public string i="預設";
        public string t = "預設";
        public string w = "預設";
    }
    class Program
    {
        static void Main(string[] args)
        {
            //懷疑是bug的事項 和未做事情
            //1.不管怎麼算 <S>都沒超過54710 根據中斷點自動變數 標題54710個 所以至少應該要有54710個<S> 但是只有一萬出頭
            //2.正則表達式必須繼續改進 考慮更多狀況 或是簡化整合式子
            //3.考慮平滑化加1係數調整至0.1或0.01之類
            //4.是否能加入等待畫面?
            //5.能否寫好後 換成後端asp.net網頁執行?(考慮對方沒有.net框架)




            if (File.Exists(@"..\..\num.txt") && File.Exists(@"..\..\cw.txt") && File.Exists(@"..\..\cw2.txt"))
            {
                Console.WriteLine("偵測到存在次數字典與字數");
            }
            else
            {
                Console.WriteLine("偵測到不存在次數字典與字數，開始建立");
                pre();
                Console.WriteLine("次數字典與字數建立完成");
            }


            while (true)
            {
                Console.WriteLine("請輸入使用法(-1離開)(1 unigram)(2 bigram):");
                string t = Console.ReadLine();
                if (t != "-1")
                {
                    if (t == "1")
                    {
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
                    else if (t == "2")
                    {
                        Console.WriteLine("使用bigram");
                        while (true)
                        {
                            Console.WriteLine("請輸入句子(-1離開):");
                            string s = Console.ReadLine();


                            if (s != "-1")
                            {
                                bigram(s);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                }
                else 
                { 
                    break; 
                }
            }

            
        }

        static void unigram(string  hh)
        {

            string pattern =
               @"[^\"" ]+(?=\;\ |\:\ |\,\""|\.\""|\!\""|\?\""|,\ |\!\ |\?\ |\.\ |\.$|\!$|\?$|\.\n|\!\n|\?\n)"//抓後面標點符號的字詞
             + @"|(?<=\ |\=)[^\"" \=]+(?=\ |\=)"//抓前面有空白或等於 後面也是空白或等於的字詞
             + @"|(?<=\"")[^\"" \n]+(?=\ )"//抓前面有雙引號後面有空白的字詞
             + @"|(?<= )[^\"" ]+(?=\"")"//抓前面有空白後面有雙引號的字詞
             + @"|^[^\"" ]+(?=\ )"//抓開頭第一個 後面有空白的字詞
             + @"|[\.,\?\!\""\:\;\=]"//標點符號抓住
             + @"|(?<=\n)[^ \""]+(?=\ )"//抓前面有換行符號後面有空白的字詞 其實可以不用 這是自己測試文章的格式需要
             + @"|(?<=\"")[^ \""]+(?=\"")";//抓前面有雙引號後面也有雙引號的字詞
            //https://regex101.com/r/xR0KDO/3

            int testwordcount = 0;
            string[] testword=new string[10000];
            foreach (Match m in Regex.Matches("<s> " + hh, pattern))
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
                    temp2 += Math.Log10(((double)(0.5)) / ((double)(num )+ (double)(0.5)*(double)(res.Count)));//(1) / (N+V)
                }
                else
                {
                    temp2 += Math.Log10(((double)(res[testword[i]]) + (double)(0.5)) / ((double)(num) + (double)(0.5)*(double)(res.Count)));//(c(wx)+1) / (N+V)
                }
                prolog += temp2;
                Console.WriteLine("{0,-100}{1,-100}", testword[i], temp2);
            }
            Console.WriteLine("\n{0,-100}{1,-100}\n\n", "The sum of all probabilities", prolog);
        }

        static void bigram(string hh)
        {
            string pattern =
               @"[^\"" ]+(?=\;\ |\:\ |\,\""|\.\""|\!\""|\?\""|,\ |\!\ |\?\ |\.\ |\.$|\!$|\?$|\.\n|\!\n|\?\n)"//抓後面標點符號的字詞
             + @"|(?<=\ |\=)[^\"" \=]+(?=\ |\=)"//抓前面有空白或等於 後面也是空白或等於的字詞
             + @"|(?<=\"")[^\"" \n]+(?=\ )"//抓前面有雙引號後面有空白的字詞
             + @"|(?<= )[^\"" ]+(?=\"")"//抓前面有空白後面有雙引號的字詞
             + @"|^[^\"" ]+(?=\ )"//抓開頭第一個 後面有空白的字詞
             + @"|[\.,\?\!\""\:\;\=]"//標點符號抓住
             + @"|(?<=\n)[^ \""]+(?=\ )"//抓前面有換行符號後面有空白的字詞 其實可以不用 這是自己測試文章的格式需要
             + @"|(?<=\"")[^ \""]+(?=\"")";//抓前面有雙引號後面也有雙引號的字詞
            //https://regex101.com/r/xR0KDO/3

            

            int testwordcount = 0;
            string[] testword = new string[10000];

            foreach (Match m in Regex.Matches("<s> "+hh, pattern))
            {
                testword[testwordcount] = m.Value;
                testwordcount++;

            }


            string[] temp = File.ReadAllLines(@"..\..\num.txt");
            int num = Int32.Parse(temp[0]);
            var res = File
                .ReadAllLines(@"..\..\cw.txt")
                .Select(l => l.Split('~'))//抓到key value
                .ToDictionary(a => a[0], a => Int32.Parse(a[1]));//轉成新字典

            var res2 = File
                .ReadAllLines(@"..\..\cw2.txt")
                .Select(l => l.Split('^'))//抓到key value
                .ToDictionary(a => a[0], a => Int32.Parse(a[1]));//轉成新字典

            double prolog = 0;
            Console.WriteLine("\n{0,-100}{1,-100}\n\n", "word", "Log10 probability");
            int cc1 = -1;
            string [] biword1 = new string[2];
            for (int i = 0; i < testwordcount; i++)
            {

                cc1++;
                biword1[cc1] = testword[i];
                if (cc1 == 1)
                {
                    cc1 = -1;
                    double temp2 = 0;
                    int actualValue = 0;
                    if (!res2.TryGetValue(biword1[0] + "~" + biword1[1], out actualValue))
                    {
                        if (res.TryGetValue(biword1[0], out actualValue))
                        {
                            temp2 += Math.Log10(
                                    (
                                        (double)(0.5)
                                    ) 
                                            / 
                                    (
                                        (double)(res[biword1[0]]) + (double)(0.5) * (double)(res.Count)
                                    )
                                );//(1) / (c(wn-1)+V) 不算太糟的情形
                        }
                        else 
                        {
                            temp2 += Math.Log10(
                                    (
                                        (double)(0.5)
                                    )
                                            / 
                                    (
                                        (double)(0.5) * (double)(res.Count)
                                    )
                                );//(1) / (V) 最糟的情形
                        }
                    }
                    else
                    {
                        temp2 += Math.Log10(
                                (
                                    (double)(res2[biword1[0] + "~" + biword1[1]]) + (double)(0.5)
                                )
                                        / 
                                (
                                    (double)(res[biword1[0]]) + (double)(0.5) * (double)(res.Count))
                                );//(1+c(wn-1,wn)) / (c(wn-1)+V) 最好的情形
                    }
                    prolog += temp2;
                    Console.WriteLine("{0,-100}{1,-100}", biword1[0] + "~" + biword1[1], temp2);

                    cc1++;
                    biword1[cc1] = testword[i];
                }                
            }
            Console.WriteLine("\n{0,-100}{1,-100}\n\n", "The sum of all probabilities", prolog);
        }


        static void pre() //前置處理 產生單字出現次數 總字數
        {
            string pattern =
               @"[^\"" ]+(?=\;\ |\:\ |\,\""|\.\""|\!\""|\?\""|,\ |\!\ |\?\ |\.\ |\.$|\!$|\?$|\.\n|\!\n|\?\n)"//抓後面標點符號的字詞
             + @"|(?<=\ |\=)[^\"" \=]+(?=\ |\=)"//抓前面有空白或等於 後面也是空白或等於的字詞
             + @"|(?<=\"")[^\"" \n]+(?=\ )"//抓前面有雙引號後面有空白的字詞
             + @"|(?<= )[^\"" ]+(?=\"")"//抓前面有空白後面有雙引號的字詞
             + @"|^[^\"" ]+(?=\ )"//抓開頭第一個 後面有空白的字詞
             + @"|[\.,\?\!\""\:\;\=]"//標點符號抓住
             + @"|(?<=\n)[^ \""]+(?=\ )"//抓前面有換行符號後面有空白的字詞 其實可以不用 這是自己測試文章的格式需要
             + @"|(?<=\"")[^ \""]+(?=\"")";//抓前面有雙引號後面也有雙引號的字詞
            //https://regex101.com/r/xR0KDO/3

            string[] lines = System.IO.File.ReadAllLines(@"..\..\ohsumed.87");
            int status = 0; int cou = -1;
            article[] tt = new article[99999];
            int cou1 = 0, cou2 = 0, cou3 = 0, cou4=0;
            int titlecou = 0, abcou = 0;

            foreach (string line in lines)//先從row data讀出標題和摘要到物件陣列 以方便操作
            {
                if (status == 1)
                {
                    cou4++;
                    tt[cou] = new article();
                    tt[cou].i = line;
                    status = 0;

                }
                if (status == 2)
                {
                    titlecou++;
                    tt[cou].t = line;
                    status = 0;
                }
                if (status == 3)
                {
                    abcou++;
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
            var dict5 = new Dictionary<string, int>();
            var dict6 = new Dictionary<string, int>();
            var dict7 = new Dictionary<string, int>();
            var dict8 = new Dictionary<string, int>();


            var dict9 = new Dictionary<string, int>();
            var dict10 = new Dictionary<string, int>();
            var dict11 = new Dictionary<string, int>();
            var dict12 = new Dictionary<string, int>();
            var dict13 = new Dictionary<string, int>();
            var dict14 = new Dictionary<string, int>();
            var dict15 = new Dictionary<string, int>();
            var dict16 = new Dictionary<string, int>();


            //跑多執行序統計物件陣列中的總字數和單一字詞出現字數
            int[] wordcount = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int MAX_COUNT = cou4;//0-54709

            //    //控制Thread數
            int WORKER_COUNT = 8;//我是8核心故設定8
            Thread[] workers = new Thread[WORKER_COUNT];
            int jobsCountPerWorker = MAX_COUNT / 8;
            for (int i = 0; i < WORKER_COUNT; i++)
            {
                //將全部工作切成WORKER_COUNT份，
                //分給WORKER_COUNT個Thread執行
                int st = jobsCountPerWorker * i;        
                int ed = jobsCountPerWorker * (i + 1);  
                if (ed > MAX_COUNT) ed = MAX_COUNT;     
                if (ed < MAX_COUNT && i == 7) ed = MAX_COUNT;

                workers[i] = new Thread(() =>
                {
                    Console.WriteLine("LOOP: {0:N0} - {1:N0}", st, ed);
                    var dicttemp = new Dictionary<string, int>();
                    for (int j = st; j < ed; j++)
                    {
                        if (tt[j].t != "預設")
                        {
                            int cc1 = -1;
                            int cc2 = -1;
                            int cc3 = -1;
                            int cc4 = -1;
                            int cc5 = -1;
                            int cc6 = -1;
                            int cc7 = -1;
                            int cc8 = -1;
                            string[] biword1 = new string[2];
                            string[] biword2 = new string[2];
                            string[] biword3 = new string[2];
                            string[] biword4 = new string[2];
                            string[] biword5 = new string[2];
                            string[] biword6 = new string[2];
                            string[] biword7 = new string[2];
                            string[] biword8 = new string[2];

                            foreach (Match m in Regex.Matches("<s> " + tt[j].t, pattern))
                            {

                                wordcount[st / jobsCountPerWorker]++;//算總字數

                                if (st / jobsCountPerWorker == 0)
                                {
                                    cc1 = b1(cc1, biword1, m, dict9);
                                    a1(dict1, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 1)
                                {
                                    cc2 = b1(cc2, biword2, m, dict10);
                                    a1(dict2, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 2)
                                {
                                    cc3 = b1(cc3, biword3, m, dict11);
                                    a1(dict3, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 3)
                                {
                                    cc4 = b1(cc4, biword4, m, dict12);
                                    a1(dict4, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 4)
                                {
                                    cc5 = b1(cc5, biword5, m, dict13);
                                    a1(dict5, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 5)
                                {
                                    cc6 = b1(cc6, biword6, m, dict14);
                                    a1(dict6, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 6)
                                {
                                    cc7 = b1(cc7, biword7, m, dict15);
                                    a1(dict7, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 7)
                                {
                                    cc8 = b1(cc8, biword8, m, dict16);
                                    a1(dict8, m.Value);
                                }
                            }
                        }
                        if (tt[j].w != "預設") 
                        {
                            int cc1 = -1;
                            int cc2 = -1;
                            int cc3 = -1;
                            int cc4 = -1;
                            int cc5 = -1;
                            int cc6 = -1;
                            int cc7 = -1;
                            int cc8 = -1;
                            string [] biword1 = new string[2];
                            string [] biword2 = new string[2];
                            string [] biword3 = new string[2];
                            string [] biword4 = new string[2];
                            string [] biword5 = new string[2];
                            string [] biword6 = new string[2];
                            string [] biword7 = new string[2];
                            string [] biword8 = new string[2];

                            foreach (Match m in Regex.Matches("<s> "+tt[j].w, pattern))
                            {

                                wordcount[st / jobsCountPerWorker]++;//算總字數

                                if (st / jobsCountPerWorker == 0)
                                {
                                    cc1=b1(cc1,biword1,m,dict9);
                                    a1(dict1, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 1)
                                {
                                    cc2 = b1(cc2, biword2, m, dict10);
                                    a1(dict2, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 2)
                                {
                                    cc3 = b1(cc3, biword3, m, dict11);
                                    a1(dict3, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 3)
                                {
                                    cc4 = b1(cc4, biword4, m, dict12);
                                    a1(dict4, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 4)
                                {
                                    cc5 = b1(cc5, biword5, m, dict13);
                                    a1(dict5, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 5)
                                {
                                    cc6 = b1(cc6, biword6, m, dict14);
                                    a1(dict6, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 6)
                                {
                                    cc7 = b1(cc7, biword7, m, dict15);
                                    a1(dict7, m.Value);
                                }
                                else if (st / jobsCountPerWorker == 7)
                                {
                                    cc8 = b1(cc8, biword8, m, dict16);
                                    a1(dict8, m.Value);
                                }
                            } 
                        }
                    }
                });
                workers[i].Start();
            }
            for (int i = 0; i < WORKER_COUNT; i++)
                workers[i].Join();

            var result = dict1.Concat(dict2).Concat(dict3).Concat(dict4).Concat(dict5).Concat(dict6).Concat(dict7).Concat(dict8)//串聯分工結果
                .GroupBy(d => d.Key)//依照key GroupBy合併value
                .OrderBy(d => d.Key)
                .ToDictionary(t => t.Key, t => t.Sum(d=>d.Value));//轉換成新的字典
            //合併字詞出現次數工作成果

            var result2 = dict9.Concat(dict10).Concat(dict11).Concat(dict12).Concat(dict13).Concat(dict14).Concat(dict15).Concat(dict16)//串聯分工結果
                .GroupBy(d => d.Key)//依照key GroupBy合併value
                .OrderBy(d => d.Key)
                .ToDictionary(t => t.Key, t => t.Sum(d => d.Value));//轉換成新的字典
            //合併字詞出現雙次數工作成果

            foreach (var te in result)
            {
                WriteLog(te.Key + "~" + te.Value);
                //寫字詞出現次數檔案
            }
            foreach (var te in result2)
            {
                WriteTextAsync(te.Key + "^" + te.Value);
                //寫雙字詞出現次數檔案
            }
            StreamWriter outputFile = new StreamWriter(@"..\..\num.txt", true);
            outputFile.WriteLine((wordcount[0] + wordcount[1] + wordcount[2] + wordcount[3] + wordcount[4] + wordcount[5] + wordcount[6] + wordcount[7]));//寫總字數檔案
            outputFile.Close();
        }

        static int b1(int cc1, string[] biword1, Match m, Dictionary<string, int> dict5)//字典雙單字記數
        {
        
            cc1++;
            biword1[cc1] = m.Value;
            if (cc1 == 1)
            {
                cc1 = -1;
                a1(dict5, biword1[0] + "~" + biword1[1]);

                cc1++;
                biword1[cc1] = m.Value;
            }
            return cc1;
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
