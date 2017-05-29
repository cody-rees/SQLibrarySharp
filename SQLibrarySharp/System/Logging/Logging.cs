using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLibrary.System.Logging {

    public class Logger {

        private readonly String Name;

		public Level LastThrownLevel;
		public String LastThrownMessage;
		public Exception LastThrownException;

        public Logger(String name) {
            this.Name = name;
        }

        public void Info(String message) {
            Info(message, null);
        }

        public void Info(String message, Exception e) {
            Log(Level.INFO, message, e);
        }

        public void Warning(String message) {
            Warning(message, null);
        }

        public void Warning(String message, Exception e) {
            Log(Level.WARNING, message, e);
        }

        public void Severe(String message) {
            Severe(message, null);
        }

        public void Severe(String message, Exception e) {
            Log(Level.SERVERE, message, e);
        }

        public void Log(Level level, String message) {
            Log(level, message, null);
        }

        public void Log(Level level, String message, Exception e) {
			this.LastThrownMessage = message;
			this.LastThrownLevel = level;
			this.LastThrownException = e;

            Console.WriteLine(Name + " | " + level.ToString() + ": " + message);
            if (e == null) {
                return;
            }

            Console.WriteLine(e.ToString());
        }

    }

    public enum Level {
        INFO, WARNING, SERVERE
    }
}
