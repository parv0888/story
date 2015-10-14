﻿namespace Story.Core
{
    using global::Story.Core.Utils;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static class StoryExtensions
    {
        /// <summary>
        /// Invokes the task to be observed by this story.
        /// </summary>
        /// <typeparam name="T">The task result type.</typeparam>
        /// <param name="func">Function returning a task to observe.</param>
        /// <returns>The result.</returns>
        public static T Run<T>(this IStory story, Func<IStory, T> func)
        {
            var tcs = new TaskCompletionSource<T>();

            try
            {
                story.Start();

                var result = func(story);
                tcs.SetResult(result);

                return result;
            }
            catch (Exception exception)
            {
                tcs.SetException(exception);
                throw;
            }
            finally
            {
                story.Stop(tcs.Task);
            }
        }

        /// <summary>
        /// Invokes the task to be observed by this story.
        /// </summary>
        /// <param name="action">Action to observe.</param>
        public static void Run(this IStory story, Action<IStory> action)
        {
            try
            {
                story.Start();

                action(story);
            }
            catch (Exception exception)
            {
                story.Data["exception"] = exception;
                throw;
            }
            finally
            {
                story.Stop();
            }
        }

        /// <summary>
        /// Invokes the task to be observed by this story.
        /// </summary>
        /// <typeparam name="T">The task result type.</typeparam>
        /// <param name="func">Function returning a task to observe.</param>
        /// <returns>The task observed by this story.</returns>
        public static Task<T> RunAsync<T>(this IStory story, Func<IStory, Task<T>> func)
        {
            story.Start();

            Task<T> result = func(story);
            story.Data["task"] = result;
            result.ContinueWith(story.Stop, TaskContinuationOptions.ExecuteSynchronously);

            return result;
        }

        /// <summary>
        /// Invokes the task to be observed by this story.
        /// </summary>
        /// <param name="func">Function returning a task to observe.</param>
        /// <returns>The task observed by this story.</returns>
        public static Task RunAsync(this IStory story, Func<IStory, Task> func)
        {
            story.Start();

            Task result = func(story);
            result.ContinueWith(story.Stop, TaskContinuationOptions.ExecuteSynchronously);

            return result;
        }

        private static void Stop(this IStory story, Task task)
        {
            Ensure.ArgumentNotNull(task, "task");

            story.Data["task"] = task;
            if (task.IsFaulted)
            {
                story.Data["exception"] = task.Exception;
            }

            story.Stop();
        }
    }
}
