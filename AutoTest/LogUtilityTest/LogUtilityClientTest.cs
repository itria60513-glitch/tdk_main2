using LogUtility;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using NUnit.Framework.Internal;
using TDKLogUtility.Module;

namespace LogUtilityUnitTest
{
    [TestFixture]
    public class LogUtilityTest
    {
		private static string CreateTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "LogUtilityClientTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        [Test]

		public void ClearLogs_Deletes_Files_Older_Than_Retention()
		{
			// Arrange
			var tempDir = CreateTempDir();
			try
			{
				//Arrange
				var oldLog = Path.Combine(tempDir, "old.log");
				var keepLog = Path.Combine(tempDir, "keep.log");
				File.WriteAllText(oldLog, "old");
				File.WriteAllText(keepLog, "keep");
				File.SetLastWriteTime(oldLog, DateTime.Now.Date.AddDays(-31));
				File.SetLastWriteTime(keepLog, DateTime.Now.Date.AddDays(-5));
				var client = PrivateAccessor.CreateUninitialized<LogUtilityClient>();
				PrivateAccessor.SetField(client, "bufferSize", -1);
				PrivateAccessor.SetField(client, "mainDirectory", tempDir);
				PrivateAccessor.SetField(client, "siNodaysBeforeToday", 30);

				// Act
				client.ClearLogs();

				// Assert
				Assert.False(File.Exists(oldLog), "old .log file should be deleted.");
				Assert.True(File.Exists(keepLog), "Log file should not be deleted in keeping days.");
			}
			finally
			{
				try { Directory.Delete(tempDir, true); } catch { /* ignore */ }
			}
		}

        [Test]
        public void GetUniqueInstanceWriteLogWithSecured_WithFakeCondition_ExceptionNotThrow()
        {
            //Arrange
            var log = LogUtilityClient.GetUniqueInstance("", 0);
            var dir = CreateTempDir();
            log.MainDirectory = dir;
            var ht = new Hashtable();
            ht.Add("test", "test");
            log.ActiveLogList = ht;

            //Assert
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    //Act
                    log.WriteLogWithSecured("TEST", LogHeadType.Info,LogCateType.None,"test",new string[]{""});
                });
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* ignore */ }
            }
        }
        [Test]
        public void GetUniqueInstanceWriteLog_WithFiveCondition_ExceptionNotThrow()
        {
            //Arrange
            var log = LogUtilityClient.GetUniqueInstance("", 0);
            var dir = CreateTempDir();
            log.MainDirectory = dir;
            var ht = new Hashtable();
            ht.Add("test", "test");
            log.ActiveLogList = ht;

            //Assert
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    //Act
                    log.WriteLog("TEST", LogHeadType.Info, LogCateType.None, "test");
                });
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* ignore */ }
            }
        }
        [Test]
        public void GetUniqueInstanceWriteLogWithSecured_WithoutCategoryCondition_ExceptionNotThrow()
        {
            //Arrange
            var log = LogUtilityClient.GetUniqueInstance("", 0);
            var dir = CreateTempDir();
            log.MainDirectory = dir;
            var ht = new Hashtable();
            ht.Add("test", "test");
            log.ActiveLogList = ht;

            //Assert
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    //Act
                    log.WriteLogWithSecured("TEST", LogHeadType.Info, "test", new string[] { "" });
                });
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* ignore */ }
            }
        }

        [Test]
        public void GetUniqueInstanceWriteLog_WithFakeCondition_ExceptionNotThrow()
        {
            //Arrange
            var log = LogUtilityClient.GetUniqueInstance("", 0);
            var dir = CreateTempDir();
            log.MainDirectory = dir;
            var ht = new Hashtable();
            ht.Add("test","test");
            log.ActiveLogList = ht;

            //Assert
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    //Act
                    log.WriteLog("TEST", "test");
                });
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* ignore */ }
            }
        }
        [Test]
        public void ClearLogs_NoCrash_When_Directory_Empty()
        {
            //Arrange
            var tempDir = CreateTempDir();
            try
            {
                var client = PrivateAccessor.CreateUninitialized<LogUtilityClient>();
                PrivateAccessor.SetField(client, "bufferSize", -1);
                PrivateAccessor.SetField(client, "mainDirectory", tempDir);
                PrivateAccessor.SetField(client, "siNodaysBeforeToday", 7);

                //Assert
                Assert.DoesNotThrow(() =>
                    {
                        //Act
                        client.ClearLogs();
                    });
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { /* ignore */ }
            }
        }



        [Test]
        public void WriteData_Formats_Ascii_And_Hex_Correctly()
        {
            // Arrange
            var client = LogUtilityClientTest.Create();
            byte[] data = new byte[] { 0x41, 0x20, 0x21, 0x01, 0x7F };

            // Act
            client.WriteData("TEST", "Payload:", data, data.Length);

            // Assert
            Assert.That(client.Captured.Count, Is.EqualTo(1), "Capture call WriteLog once");
            var entry = client.Captured[0];

            Assert.That(entry.Key, Is.EqualTo("TEST"));
            Assert.That(entry.Type, Is.EqualTo(LogHeadType.Data));
            StringAssert.Contains("Payload:", entry.Message);
            StringAssert.Contains("A !", entry.Message);
            StringAssert.Contains("(1)", entry.Message); 
            StringAssert.Contains("(7F)", entry.Message);
            StringAssert.Contains("Length:5", entry.Message);
        }



        [Test]
        public void WriteData_Respects_Length_Parameter()
        {
            //Arrange
            var client = LogUtilityClientTest.Create();
            byte[] data = new byte[] { 0x41, 0x42, 0x43 };

            //Act
            client.WriteData("TEST", "Msg:", data, 2);

            //Assert
            Assert.That(client.Captured.Count, Is.EqualTo(1));
            var msg = client.Captured[0].Message;
            StringAssert.Contains("AB", msg);
            StringAssert.DoesNotContain("C", msg);
            StringAssert.Contains("Length:2", msg);
        }



        [Test]
        public void AutoFlushTimerMinutes_Updates_Timer_Interval()
        {
            //Arrange
            var client = PrivateAccessor.CreateUninitialized<LogUtilityClient>();
            PrivateAccessor.SetField(client, "bufferSize", -1);
            var timer = new System.Timers.Timer();
            //Act
            PrivateAccessor.SetField(client, "TimeForceWrites", timer);
            client.AutoFlushTimerMinutes = 2;
            //Assert
            Assert.That(timer.Interval, Is.EqualTo(2 * 60000));
        }


    }

    internal class LogUtilityClientTest : LogUtilityClient
    {

        public List<(string Key, LogHeadType Type, LogCateType Cate, string Message, string Remark)> Captured;
        public LogUtilityClientTest() : base("", 0) { }

        public static LogUtilityClientTest Create()
        {
            var obj = (LogUtilityClientTest)FormatterServices.GetUninitializedObject(typeof(LogUtilityClientTest));
            obj.Captured = new List<(string, LogHeadType, LogCateType, string, string)>();
            return obj;
        }

        public override bool WriteLog(string szLogKey, LogHeadType enLogType, string szLogMessage, string szRemark = null)
        {
            Captured.Add((szLogKey, enLogType, LogCateType.None, szLogMessage, szRemark));

            return true;
        }


        public new void WriteLog(string szLogKey, LogHeadType enLogType, LogCateType enCateType, string szLogMessage, string szRemark = null)
        {
            Captured.Add((szLogKey, enLogType, enCateType, szLogMessage, szRemark));
        }
    }


    public static class PrivateAccessor
	{
		public static T CreateUninitialized<T>() where T : class
			=> (T)FormatterServices.GetUninitializedObject(typeof(T));

		public static void SetField<T>(T target, string fieldName, object value)
		{
			var f = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (f == null)
				throw new MissingFieldException(typeof(T).FullName, fieldName);
			f.SetValue(target, value);
		}

		public static object GetField<T>(T target, string fieldName)
		{
			var f = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (f == null)
				throw new MissingFieldException(typeof(T).FullName, fieldName);
			return f.GetValue(target);
		}
	}
}
