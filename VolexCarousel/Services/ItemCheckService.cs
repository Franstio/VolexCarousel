using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VolexCarousel.Core.Interfaces;
using VolexCarousel.Models;

namespace VolexCarousel.Services
{
    public class ItemCheckService : BackgroundService
    {
        private readonly ILogger<ItemCheckService> _logger;
        private readonly CarouselRepositoryService carouselRepositoryService;
        private ChannelReader<DateTime> reader;
        private ChannelWriter<DateTime> writer;
        private ChannelWriter<ShiftTransactionRecord> itemWriter;
        public ItemCheckService( 
            ILogger<ItemCheckService> logger,
            CarouselRepositoryService carouselRepositoryService,
            [FromKeyedServices("sensorChannel")] Channel<DateTime> sensorReader,
            [FromKeyedServices("boxTimeChannel")] Channel<DateTime> boxChannel,
            [FromKeyedServices("itemChannel")] Channel<ShiftTransactionRecord> _itemWriter)
        {
            _logger = logger;
            reader = sensorReader.Reader;
            writer = boxChannel.Writer;
            itemWriter = _itemWriter.Writer;
            this.carouselRepositoryService = carouselRepositoryService;
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await foreach (var time in reader.ReadAllAsync(stoppingToken))
            {
                try
                {


                    var shifts = await carouselRepositoryService.GetShift();
                    if (shifts is null || !shifts.Any()) continue;
                    var shift = shifts.
                        FirstOrDefault(x =>
                        {
                            if (x.shiftstart < x.shiftend)
                                return x.shiftstart <= DateTime.Now.TimeOfDay && x.shiftend >= DateTime.Now.TimeOfDay;
                            else
                                return x.shiftstart <= DateTime.Now.TimeOfDay || x.shiftend >= DateTime.Now.TimeOfDay;
                        });
                    if (shift is null) continue;
                    var ShiftTransactionRecord = new Models.ShiftTransactionRecord()
                    {
                        shiftname = shift.shiftname,
                        uid = Guid.NewGuid(),
                        targetoutput = shift.targetoutput,
                        targetdailyoutput = shift.targetdailyoutput,
                        datetimeinput = time,
                        datetimeoutput = time,
                    };
                    await writer.WriteAsync(time, stoppingToken);
                    await itemWriter.WriteAsync(ShiftTransactionRecord, stoppingToken);
                    await carouselRepositoryService.RecordItemInput(ShiftTransactionRecord);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message + " | " + e.StackTrace);
                }
            }
        }
    }
}
