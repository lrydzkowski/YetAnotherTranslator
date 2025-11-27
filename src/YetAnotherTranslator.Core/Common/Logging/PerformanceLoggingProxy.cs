using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace YetAnotherTranslator.Core.Common.Logging;

public class PerformanceLoggingProxy<T> : DispatchProxy where T : class
{
    private ILogger _logger;
    private T _target;

    public static T Create(T target, ILogger logger)
    {
        PerformanceLoggingProxy<T>? proxy = Create<T, PerformanceLoggingProxy<T>>() as PerformanceLoggingProxy<T>;
        if (proxy is not T typedProxy)
        {
            throw new InvalidOperationException("Could not create proxy instance.");
        }

        proxy?._target = target;
        proxy?._logger = logger;

        return typedProxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod is null)
        {
            throw new ArgumentNullException(nameof(targetMethod));
        }

        Type returnType = targetMethod.ReturnType;
        if (returnType == typeof(Task))
        {
            return InvokeAsyncMethod(targetMethod, args);
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return InvokeAsyncMethodWithResult(targetMethod, args);
        }

        return InvokeSyncMethod(targetMethod, args);
    }

    private async Task InvokeAsyncMethod(MethodInfo targetMethod, object?[]? args)
    {
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            Task? task = targetMethod.Invoke(_target, args) as Task;
            if (task is null)
            {
                throw new InvalidOperationException("Method invocation did not return a Task.");
            }

            await task.ConfigureAwait(false);
            sw.Stop();

            _logger.LogInformation(
                "{ClassName}.{Method} completed in {ElapsedMs}ms",
                typeof(T).Name,
                targetMethod.Name,
                sw.ElapsedMilliseconds
            );
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "{ClassName}.{Method} failed after {ElapsedMs}ms",
                typeof(T).Name,
                targetMethod.Name,
                sw.ElapsedMilliseconds
            );

            throw;
        }
    }

    private object? InvokeAsyncMethodWithResult(MethodInfo targetMethod, object?[]? args)
    {
        Type resultType = targetMethod.ReturnType.GetGenericArguments()[0];
        MethodInfo? wrapperMethod = typeof(TaskWrapper)
            .GetMethod(nameof(TaskWrapper.WrapAsync), BindingFlags.Public | BindingFlags.Static)
            ?.MakeGenericMethod(resultType);
        if (wrapperMethod is null)
        {
            throw new InvalidOperationException("Could not create wrapper method.");
        }

        return wrapperMethod.Invoke(null, [targetMethod, _target, args, _logger, typeof(T).Name]);
    }

    private object? InvokeSyncMethod(MethodInfo targetMethod, object?[]? args)
    {
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            object? result = targetMethod.Invoke(_target, args);
            sw.Stop();

            _logger.LogInformation(
                "{ClassName}.{Method} completed in {ElapsedMs}ms",
                typeof(T).Name,
                targetMethod.Name,
                sw.ElapsedMilliseconds
            );

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "{ClassName}.{Method} failed after {ElapsedMs}ms",
                typeof(T).Name,
                targetMethod.Name,
                sw.ElapsedMilliseconds
            );

            throw;
        }
    }
}

internal static class TaskWrapper
{
    public static async Task<TResult> WrapAsync<TResult>(
        MethodInfo targetMethod,
        object target,
        object?[]? args,
        ILogger logger,
        string className
    )
    {
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            Task<TResult>? task = targetMethod.Invoke(target, args) as Task<TResult>;
            if (task is null)
            {
                throw new InvalidOperationException("Method invocation did not return a Task<T>.");
            }

            TResult result = await task.ConfigureAwait(false);
            sw.Stop();

            logger.LogInformation(
                "{ClassName}.{Method} completed in {ElapsedMs}ms",
                className,
                targetMethod.Name,
                sw.ElapsedMilliseconds
            );

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "{ClassName}.{Method} failed after {ElapsedMs}ms",
                className,
                targetMethod.Name,
                sw.ElapsedMilliseconds
            );

            throw;
        }
    }
}

public abstract class PerformanceLoggingCategory
{
}
