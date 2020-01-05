﻿using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SoftLock.Tests
{
    public interface IMockInterfaceTest
    {
        void NonBlockCodeOne();
        void NonBlockCodeTwo();
        void BlockCode();
    }

    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void OnSoftLock_GuaranteeNotLock()
        {
            var softLock = new SoftLock();

            var stringBuilder = new StringBuilder();

            var mock = new Mock<IMockInterfaceTest>();
            mock.Setup(x => x.NonBlockCodeOne())
                .Callback(() =>
                {
                    softLock.EnterSoftLock();
                    if (softLock.InBlockCode)
                    {
                        stringBuilder.Append("Error: NonBlockCodeOne() until InBlockCode\n");
                    }

                    // Some code

                    if (softLock.InBlockCode)
                    {
                        stringBuilder.Append("Error: NonBlockCodeOne() until InBlockCode\n");
                    }
                    softLock.ExitSoftLock();
                });
            mock.Setup(x => x.NonBlockCodeTwo())
                .Callback(() =>
                {
                    softLock.EnterSoftLock();
                    if (softLock.InBlockCode)
                    {
                        stringBuilder.Append("Error: NonBlockCodeTwo() until InBlockCode\n");
                    }

                    // Some code

                    if (softLock.InBlockCode)
                    {
                        stringBuilder.Append("Error: NonBlockCodeTwo() until InBlockCode\n");
                    }
                    softLock.ExitSoftLock();
                });
            mock.Setup(x => x.BlockCode())
                .Callback(() =>
                {
                    softLock.EnterHardLock();
                    if (!softLock.InBlockCode)
                    {
                        stringBuilder.Append("Error: BlockCode() until InBlockCode flag is false\n");
                    }

                    // Some code

                    if (!softLock.InBlockCode)
                    {
                        stringBuilder.Append("Error: BlockCode() until InBlockCode flag is false\n");
                    }
                    softLock.ExitHardLock();
                });


            var obj = mock.Object;
            var qtdTest = 10000;
            Task.WaitAll(
                Task.Factory.StartNew(() =>
                {
                    Thread.CurrentThread.Name = "NonBlockCodeOne()";
                    ExecTimes(qtdTest, obj.NonBlockCodeOne);
                }),
                Task.Factory.StartNew(() =>
                {
                    Thread.CurrentThread.Name = "NonBlockCodeTwo()";
                    ExecTimes(qtdTest, obj.NonBlockCodeTwo);
                }),
                Task.Factory.StartNew(() =>
                {
                    Thread.CurrentThread.Name = "BlockCode()";
                    ExecTimes(qtdTest, obj.BlockCode);
                })
            );

            Assert.AreEqual(string.Empty, stringBuilder);
        }

        private void ExecTimes(int times, Action cb)
        {
            foreach (var _ in Enumerable.Range(1, times))
            {
                cb?.Invoke();
            }
        }
    }
}
