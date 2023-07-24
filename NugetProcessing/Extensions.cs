using System.Threading.Tasks.Dataflow;

static class Extensions
{
    public static void Dump(this string str)
    {
        Console.WriteLine(str);
    }
}

public static class CollectionExtensions
{
    /// <summary>
    /// An ParallelForEach with an optional ConditionDegreeOfParallelism so we don't exhaust all threads, Uses the TPL to process the items in parallel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elements"></param>
    /// <param name="action"></param>
    /// <param name="parallelism">Defaults to 8 or the numbers of processors if greater than 8</param>
    /// <example>
    ///  var e = Enumerable.Range(0, 100);
    ///  await e.ParallelForEach((i) =&gt; await Process(i); );
    /// </example>
    /// <returns></returns>
    public static async Task ParallelForEach<T>(this IEnumerable<T> elements, Func<T, Task> action,
        int parallelism = -1)
    {
        if (elements == null || !elements.Any())
        {
            return;
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var actionBlock = new ActionBlock<T>(async d => await action(d),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = ConditionDegreeOfParallelism(parallelism) });

        foreach (var element in elements)
        {
            actionBlock.Post(element);
        }

        actionBlock.Complete();
        await actionBlock.Completion;
    }
    /// <summary>
    /// An ParallelForEach with an optional ConditionDegreeOfParallelism so we don't exhaust all threads, Uses the TPL to process the items in parallel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elements"></param>
    /// <param name="action"></param>
    /// <param name="parallelism"></param>
    /// <example>
    ///  var e = Enumerable.Range(0, 100);
    ///  await e.ParallelForEach((i) =&gt; Process(i); );
    /// </example>
    /// <returns></returns>
    public static async Task ParallelForEach<T>(this IEnumerable<T> elements, Action<T> action,
        int parallelism = -1)
    {
        await elements.ParallelForEach((f) =>
        {
            action(f);
            return Task.CompletedTask;
        });
    }

    private static int ConditionDegreeOfParallelism(int degreeOfParallelism)
    {
        if (degreeOfParallelism > 0)
        {
            return degreeOfParallelism;
        }

        var processorCount = Environment.ProcessorCount - 1;
        return Math.Max(8, Math.Max(2, processorCount));
    }
}