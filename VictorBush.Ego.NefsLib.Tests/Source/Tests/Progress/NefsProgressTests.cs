// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Tests.NefsLib.Progress
{
    using System;
    using System.Threading;
    using VictorBush.Ego.NefsLib.Progress;
    using Xunit;

    public class NefsProgressTests
    {
        [Fact]
        public void Test_SingleTask()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            p.BeginTask(1.0f);
            Assert.Equal(0.0f, p.Percent);
            Assert.Equal("", p.StatusMessage);
            Assert.Equal("", p.StatusSubMessage);

            p.EndTask();
            Assert.Equal(1.0f, p.Percent);
            Assert.Equal("", p.StatusMessage);
            Assert.Equal("", p.StatusSubMessage);
        }


        [Fact]
        public void Test_MultipleTasks()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            p.BeginTask(1.0f);
            p.BeginTask(0.5f);
            Assert.Equal(0.0f, p.Percent);

            p.EndTask();
            Assert.Equal(0.5f, p.Percent);

            p.EndTask();
            Assert.Equal(1.0f, p.Percent);
        }

        [Fact]
        public void Test_MultipleTasksWithMessage()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            p.BeginTask(1.0f, "A");
            Assert.Equal(0.0f, p.Percent);
            Assert.Equal("A", p.StatusMessage);
            Assert.Equal("", p.StatusSubMessage);

            p.BeginTask(0.25f, "B");
            Assert.Equal(0.0f, p.Percent);
            Assert.Equal("B", p.StatusMessage);
            Assert.Equal("", p.StatusSubMessage);

            p.EndTask();
            Assert.Equal(0.25f, p.Percent);

            p.EndTask();
            Assert.Equal(1.0f, p.Percent);
        }

        [Fact]
        public void Test_MultipleWithSubTasks()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            p.BeginTask(1.0f, "A");
            Assert.Equal(0.0f, p.Percent);
            Assert.Equal("A", p.StatusMessage);
            Assert.Equal("", p.StatusSubMessage);
            {
                p.BeginTask(0.2f);
                Assert.Equal(0.0f, p.Percent);
                Assert.Equal("A", p.StatusMessage);
                Assert.Equal("", p.StatusSubMessage);
                {
                    p.BeginSubTask(0.5f, "sub1");
                    Assert.Equal(0.0f, p.Percent);
                    Assert.Equal("A", p.StatusMessage);
                    Assert.Equal("sub1", p.StatusSubMessage);

                    p.EndTask();
                    Assert.Equal(0.1f, p.Percent);
                    Assert.Equal("A", p.StatusMessage);
                    Assert.Equal("", p.StatusSubMessage);

                    p.BeginSubTask(0.5f, "sub2");
                    Assert.Equal(0.1f, p.Percent);
                    Assert.Equal("A", p.StatusMessage);
                    Assert.Equal("sub2", p.StatusSubMessage);

                    p.EndTask();
                    Assert.Equal(0.2f, p.Percent);
                    Assert.Equal("A", p.StatusMessage);
                    Assert.Equal("", p.StatusSubMessage);
                }
                p.EndTask();
                Assert.Equal(0.2f, p.Percent);
                Assert.Equal("A", p.StatusMessage);
                Assert.Equal("", p.StatusSubMessage);

                p.BeginTask(0.8f, "B");
                Assert.Equal(0.2f, p.Percent);
                Assert.Equal("B", p.StatusMessage);
                Assert.Equal("", p.StatusSubMessage);

                p.EndTask();
                Assert.Equal(1.0f, p.Percent);
                Assert.Equal("A", p.StatusMessage);
                Assert.Equal("", p.StatusSubMessage);
            }
            p.EndTask();
            Assert.Equal(1.0f, p.Percent);
            Assert.Equal("", p.StatusMessage);
            Assert.Equal("", p.StatusSubMessage);
        }

        [Fact]
        public void Test_MoreTests()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            p.BeginTask(1.0f);
            {
                p.BeginTask(0.1f);
                this.Verify(p, 0.0f, "", "");
                {
                    p.BeginSubTask(0.4f, "sub");
                    this.Verify(p, 0.0f, "", "sub");
                    p.EndTask();
                    this.Verify(p, 0.04f, "", "");

                    p.BeginSubTask(0.6f, "sub");
                    this.Verify(p, 0.04f, "", "sub");
                    p.EndTask();
                    this.Verify(p, 0.1f, "", "");
                }
                p.EndTask();
                this.Verify(p, 0.1f, "", "");

                p.BeginTask(0.8f);
                this.Verify(p, 0.1f, "", "");
                p.EndTask();
                this.Verify(p, 0.9f, "", "");

                p.BeginTask(0.05f);
                this.Verify(p, 0.9f, "", "");
                p.EndTask();
                this.Verify(p, 0.95f, "", "");

                // 0.1 + 0.8 + 0.05 == 0.95 (does not add up to 1)
            }
            p.EndTask();
            this.Verify(p, 1.0f, "", "");
        }

        [Fact]
        public void BeginTask_NoMessage_ProgressChangedRaised()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            NefsProgressEventArgs args = null;
            p.ProgressChanged += (o, e) => args = e;

            p.BeginTask(1.0f);
            Assert.Equal(p.StatusMessage, args.Message);
            Assert.Equal(p.StatusSubMessage, args.SubMessage);
            Assert.Equal(p.Percent, args.Progress);
        }

        [Fact]
        public void BeginTask_Message_ProgressChangedRaised()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            NefsProgressEventArgs args = null;
            p.ProgressChanged += (o, e) => args = e;

            p.BeginTask(1.0f, "A");
            Assert.Equal(p.StatusMessage, args.Message);
            Assert.Equal(p.StatusSubMessage, args.SubMessage);
            Assert.Equal(p.Percent, args.Progress);
        }

        [Fact]
        public void BeginTask_WeightTooSamll_ArgumentOutOfRangeExceptionThrown()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);
            Assert.Throws<ArgumentOutOfRangeException>(() => p.BeginTask(-1.0f));
        }

        [Fact]
        public void BeginTask_WeightTooBig_ArgumentOutOfRangeExceptionThrown()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);
            Assert.Throws<ArgumentOutOfRangeException>(() => p.BeginTask(2.0f));
        }

        [Fact]
        public void BeginSubTask_Valid_ProgressChangedRaised()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);

            p.BeginTask(1.0f, "A");
            {
                NefsProgressEventArgs args = null;
                p.ProgressChanged += (o, e) => args = e;

                p.BeginSubTask(1.0f, "sub");
                this.Verify(p, 0.0f, "A", "sub");
                Assert.Equal("A", args.Message);
                Assert.Equal("sub", args.SubMessage);
                Assert.Equal(0.0f, args.Progress);

                p.EndTask();
                this.Verify(p, 1.0f, "A", "");
            }
            p.EndTask();
            this.Verify(p, 1.0f, "", "");
        }

        [Fact]
        public void BeginSubTask_SubTaskIsFirstTask_InvalidOperationExceptionThrown()
        {
            var ct = new CancellationTokenSource().Token;
            var p = new NefsProgress(ct);
            Assert.Throws<InvalidOperationException>(() => p.BeginSubTask(1.0f, "msg"));
        }

        private void Verify(NefsProgress p, float percent, string msg, string sub)
        {
            // Verify to 6 decimals due to floating point precision issues
            Assert.Equal(percent, p.Percent, 6);
            Assert.Equal(msg, p.StatusMessage);
            Assert.Equal(sub, p.StatusSubMessage);
        }
    }
}
