using System;
using HNZ.Utils.Logging;
using Sandbox.ModAPI;

namespace HNZ.Utils
{
    public sealed class ContentFile<T> where T : class
    {
        readonly Logger Log = LoggerManager.Create(nameof(ContentFile<T>));
        readonly string _fileName;

        public ContentFile(string fileName, T defaultContent)
        {
            _fileName = fileName;
            Content = defaultContent;
        }

        public T Content { get; set; }

        public void ReadOrCreateFile()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(_fileName, typeof(T)))
            {
                try
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(_fileName, typeof(T)))
                    {
                        var contentText = reader.ReadToEnd();
                        Content = MyAPIGateway.Utilities.SerializeFromXML<T>(contentText);
                        Log.Debug($"Loaded file: \"{_fileName}\": {Content}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return;
                }
            }

            WriteFile();
        }

        public void WriteFile()
        {
            try
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(_fileName, typeof(T)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(Content));
                    Log.Debug($"Wrote file: \"{_fileName}\": {Content}");
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
        }
    }
}