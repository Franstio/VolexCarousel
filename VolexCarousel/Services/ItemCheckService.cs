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
        private Guid uid = Guid.NewGuid();
        private readonly ICheckItemService _plcService;
        private readonly ILogger<ItemCheckService> _logger;
        private readonly string INPUT_ADDRESS = "";
        private readonly string OUTPUT_ADDRESS = "";
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
            return !string.IsNullOrEmpty(data) ? data == "OK" : false;
        }

        public async Task<bool> CheckItemOutput()
        {
            var data = await _plcService.CheckItemAsync(OUTPUT_ADDRESS);
            return !string.IsNullOrEmpty(data) ? data == "OK" : false;
        }

        public async IAsyncEnumerable<ShiftTransactionRecord> RunCheckInput([EnumeratorCancellation] CancellationToken cancelTokenSource=default)
        {
            _plcService.Start();
            while (!cancelTokenSource.IsCancellationRequested)
            {
                await Task.Delay(100);
                if (await CheckItemInput())
                {
                    if (ShiftTransactionRecord.Any() && ShiftTransactionRecord.Peek().uid == uid) continue;
                    ShiftTransactionRecord.Enqueue(new Models.ShiftTransactionRecord()
                    {
                        uid = uid,
                        datetimeinput = DateTime.Now,
                    });
                    yield return ShiftTransactionRecord.Peek();
                }
                else
                    uid = Guid.NewGuid();
            }
        }

        public async IAsyncEnumerable<ShiftTransactionRecord> RunCheckOutput([EnumeratorCancellation] CancellationToken cancelTokenSource = default)
        {
            _plcService.Start();
            while (!cancelTokenSource.IsCancellationRequested)
            {
                await Task.Delay(100);
                if (await CheckItemOutput())
                {
                    if (!ShiftTransactionRecord.Any()) continue;

                    var shifts = await carouselRepositoryService.GetShift();
                    var shift = shifts.Where(x => x.shiftstart <= DateTime.Now.TimeOfDay && x.shiftend >= DateTime.Now.TimeOfDay).FirstOrDefault();

                    if (shift is null) continue;
                    var item = ShiftTransactionRecord.Dequeue();
                    item.shiftname = shift.shiftname;
                    item.targetoutput = shift.targetoutput;
                    item.datetimeoutput = DateTime.Now;
                    yield return item;
                }
            }
        }



    }
}
