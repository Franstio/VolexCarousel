using Avalonia.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VolexCarousel.Interfaces;
using VolexCarousel.Models;

namespace VolexCarousel.Services
{
    public class ItemCheckService
    {
        private Guid uid = Guid.NewGuid(),ouid=Guid.NewGuid(),ouidcheck=Guid.NewGuid();
        private readonly ICheckItemService _plcService;
        private readonly ILogger<ItemCheckService> _logger;
        private readonly string INPUT_ADDRESS = "MR001";
        private readonly string OUTPUT_ADDRESS = "MR002";
        private DateTime _boxByBoxRecord = DateTime.Now;
        private Queue<ShiftTransactionRecord> ShiftTransactionRecord = [];
        private readonly CarouselRepositoryService carouselRepositoryService;
        public ItemCheckService(ICheckItemService tcpPLCService, ILogger<ItemCheckService> logger,
            CarouselRepositoryService carouselRepositoryService)
        {
            _plcService = tcpPLCService;
            _logger = logger;
            this.carouselRepositoryService = carouselRepositoryService;
        }


        public async Task<bool> CheckItemInput()
        {
            var data = await _plcService.CheckItemAsync(INPUT_ADDRESS);
            return !string.IsNullOrEmpty(data) ? data == "OK" || data == "1" : false;
        }

        public async Task<bool> CheckItemOutput()
        {
            var data = await _plcService.CheckItemAsync(OUTPUT_ADDRESS);
            return !string.IsNullOrEmpty(data) ? data == "OK" || data == "1" : false;
        }

        public async IAsyncEnumerable<ShiftTransactionRecord> RunCheckInput([EnumeratorCancellation] CancellationToken cancelTokenSource=default)
        {
            _plcService.Start();
            while (!cancelTokenSource.IsCancellationRequested)
            {
                ShiftTransactionRecord? record = null;
                await Task.Delay(50);
                try
                {

                    if (await CheckItemInput())
                    {
                        if (ShiftTransactionRecord.Any() && ShiftTransactionRecord.Peek().uid == uid) continue;

                        var shifts = await carouselRepositoryService.GetShift();
                        if (shifts is null || !shifts.Any()) continue;
                        var shift = shifts.
                            Where(x => {
                                if (x.shiftstart < x.shiftend)
                                    return x.shiftstart <= DateTime.Now.TimeOfDay && x.shiftend >= DateTime.Now.TimeOfDay;
                                else
                                    return x.shiftstart <= DateTime.Now.TimeOfDay || x.shiftend >= DateTime.Now.TimeOfDay;
                            }).FirstOrDefault();
                        if (shift is null) continue;
                        ShiftTransactionRecord.Enqueue(new Models.ShiftTransactionRecord()
                        {
                            shiftname = shift.shiftname,
                            uid = uid,
                            targetoutput = shift.targetoutput,
                            datetimeinput = DateTime.Now,
                        });
                        record = ShiftTransactionRecord.Peek();
                    }
                    else
                        uid = Guid.NewGuid();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message + " | " + e.StackTrace);
                }
                if (record is not null)
                    yield return ShiftTransactionRecord.Peek();
            }
        }
        public void Stop()
        {
            _plcService.Stop();
        }

        public async IAsyncEnumerable<ShiftTransactionRecord> RunCheckOutput([EnumeratorCancellation] CancellationToken cancelTokenSource = default)
        {
            _plcService.Start();
            while (!cancelTokenSource.IsCancellationRequested)
            {
                await Task.Delay(50);
                ShiftTransactionRecord? item = null;
                try
                {
                    if (await CheckItemOutput())
                    {
                        if (ouid == ouidcheck) continue;

                        if (!ShiftTransactionRecord.Any()) continue;
                        item = ShiftTransactionRecord.Dequeue();

                        ouidcheck = ouid;
                        item.datetimeoutput = DateTime.Now;
                    }
                    else
                        ouid = Guid.NewGuid();
                }
                catch(Exception e)
                {
                    _logger.LogError(e.Message + " | " + e.StackTrace);
                }
                if (item is not null)
                    yield return item;
            }
        }



    }
}
