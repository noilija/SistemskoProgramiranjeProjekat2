using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugiProjekat
{
    // ovo je element koji se cuva u shmemRed (ConcurentQUeue)
    // cuvamo ga kako bismo pravom klijentu slali odgovor (zbog contexta)
    internal class FileRequest
    {
        public FileRequest(string path, HttpListenerContext context)
        {
            this.path = path;
            this.context = context;

        }

        private string path; // path koji se hesira
        private HttpListenerContext context; // context da bismo znali kome se prosledjuje

        public string Path { get => path; set => path = value; }
        public HttpListenerContext Context { get => context; set => context = value; }
    }
}
