using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VolexCarousel.Models;

namespace VolexCarousel.Services
{
    public class ItemCheckService
    {
        private readonly TCPModbusService _modbusService;
        private readonly ILogger<ItemCheckService> _logger;
        private readonly byte INPUT_ADDRESS = 1;
        private readonly byte OUTPUT_ADDRESS = 2;
        private DateTime _boxByBoxRecord = DateTime.Now;
        private Queue<ShiftTransactionRecord> ShiftTransactionRecord = [];
        public ItemCheckService(TCPModbusService modbusService, ILogger<ItemCheckService> logger)
        {
            _modbusService = modbusService;
            _logger = logger;
        }


        public async Task<bool> CheckItemInput()
        {
            var data = await _modbusService.ReadData(INPUT_ADDRESS);
            return data.Any() ? data[0] == 1 : false;
        }

        public async Task<bool> CheckItemOutput()
        {
            var data = await _modbusService.ReadData(OUTPUT_ADDRESS);
            return data.Any() ? data[0] == 1 : false;
        }

        public async IAsyncEnumerable<ShiftTransactionRecord> RunCheckInput([EnumeratorCancellation] CancellationToken cancelTokenSource=default)
        {
            while (!cancelTokenSource.IsCancellationRequested)
            {
                if (await CheckItemInput())
                {
                    ShiftTransactionRecord.Enqueue(new Models.ShiftTransactionRecord()
                    {
                        datetimeinput = DateTime.Now,
                    });
                    yield return ShiftTransactionRecord.Peek();
                }
            }
        }

        
        
    }
}
