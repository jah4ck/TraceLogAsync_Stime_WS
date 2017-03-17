# TraceLogAsync

DLL de gestion de log avec file d'attente

> Gestion des file d'attente : 
```
private static LogWriter instance;
private static Queue<Log> logQueue;
private static int maxLogAge = 1;//age en seconde (toutes les x seconde on met à jour les log)
private static int queueSize = 4000;//nombre dans queue (toutes les x ligne on met a jour les log)
private static DateTime LastFlushed = DateTime.Now.AddMinutes(-1);
```

> Ecriture
```
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
```