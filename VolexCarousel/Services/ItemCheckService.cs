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
using VolexCarousel.Interfaces;
using VolexCarousel.Models;

namespace VolexCarousel.Services
{
    public class ItemCheckService : BackgroundService
    {
        private readonly ILogger<ItemCheckService> _logger;
        private readonly string SENSOR_ADDRESS = "R003";
        private readonly CarouselRepositoryService carouselRepositoryService;
        private ShiftTransactionRecord? ShiftTransactionRecord = null;
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
                    if (ShiftTransactionRecord is null)
                    {
                        var shift = shifts.
                            FirstOrDefault(x =>
                            {
                                if (x.shiftstart < x.shiftend)
                                    return x.shiftstart <= DateTime.Now.TimeOfDay && x.shiftend >= DateTime.Now.TimeOfDay;
                                else
                                    return x.shiftstart <= DateTime.Now.TimeOfDay || x.shiftend >= DateTime.Now.TimeOfDay;
                            });
                        if (shift is null) continue;
                        ShiftTransactionRecord = new Models.ShiftTransactionRecord()
                        {
                            shiftname = shift.shiftname,
                            uid = Guid.NewGuid(),
                            targetoutput = shift.targetoutput,
                            targetdailyoutput = shift.targetdailyoutput,
                            datetimeinput = time,
                        };
                        await writer.WriteAsync(time);
                    }
                    else
                    {
                        ShiftTransactionRecord.datetimeoutput = time;
                        await carouselRepositoryService.RecordItemInput(ShiftTransactionRecord);
                        await itemWriter.WriteAsync(ShiftTransactionRecord,stoppingToken);
                        ShiftTransactionRecord = null;
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message + " | " + e.StackTrace);
                }
            }
        }
    }
}
