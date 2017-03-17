using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace TraceLogAsync
{
    
    public class LogWriter
    {
        
        private static LogWriter instance;
        private static Queue<Log> logQueue;
        private static int maxLogAge = 1;//age en seconde (toutes les x seconde on met à jour les log)
        private static int queueSize = 4000;//nombre dans queue (toutes les x ligne on met a jour les log)
        private static DateTime LastFlushed = DateTime.Now.AddMinutes(-1);

        
        ~LogWriter()
        {
            FlushLog();
        }
        private LogWriter() 
        {
            

        }

        
       
        public static LogWriter Instance
        {
            get
            {
                
                if (instance == null)
                {
                    instance = new LogWriter();
                    logQueue = new Queue<Log>();
                }
                return instance;
            }
        }

        
        public void WriteToLog(string nameFile,string message, string  pathSortie,string FormatSortie,string Pdv, string nameListe)//(string message, int codeErreur,string codeAppli, string nameFile )
        {
           
            
            lock (logQueue)
            {
                
                Log logEntry = new Log(nameFile,message,pathSortie,FormatSortie,Pdv, nameListe);
                logQueue.Enqueue(logEntry);

                
                if (logQueue.Count >= queueSize || DoPeriodicFlush())
                {
                    FlushLog();
                }
            }            
        }

        private bool DoPeriodicFlush()
        {
            TimeSpan logAge = DateTime.Now - LastFlushed;
            if (logAge.TotalSeconds >= maxLogAge)
            {
                LastFlushed = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CheckPdvListe(string path, string ligneToModif, string ligneFinale)
        {
            string texteFinal = null;
            StreamReader sr = new StreamReader(path);
            string ligneEnCoursDeLecture = null;
            while (sr.Peek() != -1)
            {
                ligneEnCoursDeLecture = sr.ReadLine();
                if ((ligneEnCoursDeLecture == ligneToModif))
                {
                    texteFinal = (texteFinal
                                + (ligneFinale + "\r\n"));
                }
                else
                {
                    texteFinal = (texteFinal
                                + (ligneEnCoursDeLecture + "\r\n"));
                }
            }
            sr.Close();
            // Ré-écriture du fichier
            StreamWriter sr2 = new StreamWriter(path);
            sr2.WriteLine(texteFinal);
            sr2.Close();
        }
       
        public void FlushLog()
        {

            while (logQueue.Count > 0)
            {
                Log entry = logQueue.Dequeue();
                string logDir = entry.PathSortie;
                string pathListe = @"D:\DATA\LISTE\" + entry.NameListe;
                if (!Directory.Exists(entry.PathSortie))
                {
                    Directory.CreateDirectory(entry.PathSortie);
                }
                if (entry.FormatSortie=="PDV")
                {
                    Directory.CreateDirectory(entry.PathSortie+entry.PDV);
                    logDir = logDir + entry.PDV+@"\";
                }
                
                string logPath = logDir + entry.NameFile;
                if (Directory.Exists(logDir))
                {
                    using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter log = new StreamWriter(fs))
                        {
                            if (entry.Message != String.Empty)
                            {
                                if (entry.FormatSortie=="LOGWS")
                                {
                                    
                                    log.WriteLine(string.Format("{0}\t{1}", entry.LogTime, entry.Message));
                                    
                                }
                                else
                                {
                                    if (entry.Message.Length > 5)
                                    {
                                        log.WriteLine(entry.Message);
                                    }
                                }
                                
                            }
                        }
                    }
                }
                if (File.Exists(pathListe)) //Contrôle des pdv ayant répondu
                {
                    string _pdv = "";
                    if (entry.PDV.Length==4)
                    {
                        _pdv = "0" + entry.PDV;
                    }
                    else if (entry.PDV.Length==5)
                    {
                        _pdv=entry.PDV;
                    }
                    if (_pdv!="")
                    {
                        if (entry.Message.Length>5)
                        {
                            CheckPdvListe(pathListe, entry.PDV, entry.PDV + ";OK");
                        }
                        else
                        {
                            CheckPdvListe(pathListe, entry.PDV, entry.PDV + ";AUCUN RESULTAT");
                        }
                        
                    }
                }
                
            }
                        
        }
        
    }

    
    public class Log
    {
        public string Message { get; set; }
        public string NameListe { get; set; }
        public string LogTime { get; set; }
        public string LogDate { get; set; }
        public string NameFile { get; set; }
        public string PathSortie { get; set; }
        public string FormatSortie { get; set; }
        public string PDV { get; set; }


        public Log(string _nameFile, string _message, string _pathSortie, string _FormatSortie, string _Pdv, string _nameListe)
        {
            Message = _message;
            NameFile = _nameFile;
            PathSortie = _pathSortie;
            FormatSortie = _FormatSortie;
            PDV = _Pdv;
            NameListe = _nameListe;
            LogDate = DateTime.Now.ToString("yyyy-MM-dd");
            LogTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
        }
    }

    

}
