﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace Embeddinator.Tests
{
    /// <summary>
    /// This is a very simple implementation of ApprovalTests: http://approvaltests.com/
    /// - I would prefer to use ApprovalTests, but had issues running on Mono
    /// </summary>
    public static class Approvals
    {
        static readonly string basePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(Approvals).Assembly.Location), "..", "..", "Approvals"));

        static string GetPath(string sourceFile, string member, string suffix)
        {
            string fileName = string.Join(".", Path.GetFileNameWithoutExtension(sourceFile), member, suffix, "txt");
            return Path.Combine(basePath, fileName);
        }

        static List<string> ZipFiles(string filePath)
        {
            List<string> fileNames = new List<string>();
            using (var stream = File.OpenRead(filePath))
            using (var zip = new ZipArchive(stream))
            {
                foreach (var entry in zip.Entries)
                {
                    fileNames.Add(entry.FullName);
                }
            }
            return fileNames;
        }

        /// <summary>
        /// Verifies a body of text against the approved text on disk.
        /// - Make sure to call this method directly from the unit test method, so that [CallerFilePath] works properly
        /// </summary>
        public static void Verify(string text, [CallerFilePath]string sourceFile = null, [CallerMemberName]string member = null)
        {
            string received = GetPath(sourceFile, member, "received");
            string approved = GetPath(sourceFile, member, "approved");
            File.WriteAllText(received, text);
            FileAssert.AreEqual(approved, received);
            File.Delete(received);
        }

        /// <summary>
        /// Verifies an existing file against the approved text on disk.
        /// - Make sure to call this method directly from the unit test method, so that [CallerFilePath] works properly
        /// </summary>
        public static void VerifyFile(string filePath, bool ignoreNewLine = false, [CallerFilePath]string sourceFile = null, [CallerMemberName]string member = null)
        {
            string received = GetPath(sourceFile, member, "received");
            string approved = GetPath(sourceFile, member, "approved");
            File.Copy(filePath, received, true);

            if (ignoreNewLine)
            {
                using (StreamReader receivedStream = File.OpenText(received))
                using (StreamReader approvedStream = File.OpenText(approved))
                {
                    while (!receivedStream.EndOfStream)
                    {
                        if (receivedStream.ReadLine() != approvedStream.ReadLine()) Assert.Fail();
                        if (receivedStream.EndOfStream != approvedStream.EndOfStream) Assert.Fail();
                    }
                    Assert.Pass();
                }
            }
            else
            {
                FileAssert.AreEqual(approved, received);
            }
            File.Delete(received);
        }

        /// <summary>
        /// Verifies the list of filenames in a zip archive against the approved text on disk. Unaffected by order.
        /// - Make sure to call this method directly from the unit test method, so that [CallerFilePath] works properly
        /// </summary>
        public static void VerifyZipFile(string filePath, [CallerFilePath]string sourceFile = null, [CallerMemberName]string member = null)
        {
            IEnumerable<string> recieved = ZipFiles(filePath);
            IEnumerable<string> approved = File.ReadAllLines(GetPath(sourceFile, member, "approved"));
            CollectionAssert.AreEquivalent(approved, recieved);
        }

        /// <summary>
        /// Do not commit code using this method. It should be used temporarily to approve a test.
        /// </summary>
        [Obsolete("Do not commit code using this method. It should be used temporarily to approve a test.")]
        public static void Approve(string text, [CallerFilePath]string sourceFile = null, [CallerMemberName]string member = null)
        {
#if !DEBUG
            //Fail release builds on purpose
            Assert.Fail("This test is using Approvals.Approve() when it should be using Approvals.Verify()!");
#endif
            string approved = GetPath(sourceFile, member, "approved");
            File.WriteAllText(approved, text);
            Verify(text, sourceFile, member);
        }

        /// <summary>
        /// Do not commit code using this method. It should be used temporarily to approve a test.
        /// </summary>
        [Obsolete("Do not commit code using this method. It should be used temporarily to approve a test.")]
        public static void ApproveFile(string filePath, [CallerFilePath]string sourceFile = null, [CallerMemberName]string member = null)
        {
#if !DEBUG
            //Fail release builds on purpose
            Assert.Fail("This test is using Approvals.ApproveFile() when it should be using Approvals.VerifyFile()!");
#endif
            string approved = GetPath(sourceFile, member, "approved");
            File.Copy(filePath, approved, true);
            VerifyFile(filePath, false, sourceFile, member);
        }

        /// <summary>
        /// Do not commit code using this method. It should be used temporarily to approve a test.
        /// </summary>
        [Obsolete("Do not commit code using this method. It should be used temporarily to approve a test.")]
        public static void ApproveZipFile(string filePath, [CallerFilePath]string sourceFile = null, [CallerMemberName]string member = null)
        {
#if !DEBUG
            //Fail release builds on purpose
            Assert.Fail("This test is using Approvals.ApproveZipFile() when it should be using Approvals.VerifyZipFile()!");
#endif
            string approved = GetPath(sourceFile, member, "approved");
            File.WriteAllText(approved, string.Join(Environment.NewLine, ZipFiles(filePath)));
            VerifyZipFile(filePath, sourceFile, member);
        }
    }
}
