using System.Threading.Channels;

namespace ECommerceInventory.Infrastructure.BackgroundServices;

public record PaymentTask(Guid OrderId, decimal Amount);

public class PaymentQueue
{
    private readonly Channel<PaymentTask> _channel;

    public PaymentQueue()
    {
        _channel = Channel.CreateUnbounded<PaymentTask>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ChannelWriter<PaymentTask> Writer => _channel.Writer;
    public ChannelReader<PaymentTask> Reader => _channel.Reader;

    public async ValueTask EnqueueAsync(PaymentTask task, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(task, cancellationToken);
    }

    public bool TryEnqueue(PaymentTask task)
    {
        return _channel.Writer.TryWrite(task);
    }
}
