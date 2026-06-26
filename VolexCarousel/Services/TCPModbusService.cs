using Microsoft.Extensions.Logging;
using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolexCarousel.Services
{
    public class TCPModbusService
    {
        private  IModbusMaster modbus = null!;
        private readonly TcpClient tcpClient;
        private readonly ILogger<TCPModbusService> logger;
        private readonly AppSettingService settingService;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public TCPModbusService(IModbusMaster modbus, ILogger<TCPModbusService> logger,AppSettingService settingService)
        {
            this.modbus = modbus;
            this.logger = logger;
            this.settingService = settingService;
            this.tcpClient = new TcpClient();
        }

        public void Start()
        {
            try
            {
                if (tcpClient.Connected)
                    return;
                ModbusFactory factory = new ModbusFactory();
                string data = settingService.LoadSettings().PLCPort;
                tcpClient.Connect(data.Split(':')[0], Convert.ToInt32(data.Split(":")[1]));
                modbus = factory.CreateMaster(tcpClient);
            }

            catch (Exception e)
            {
                logger.LogError(e.Message + " | " + e.StackTrace);
            }
        }

        public void Stop()
        {
            try
            {
                modbus.Dispose();
                tcpClient.Close();
            }
            catch(Exception e)
            {
                logger.LogError(e.Message + " | " + e.StackTrace);
            }
        }

        public async Task<ushort[]> ReadData(ushort address,ushort length = 1,byte client_id=1)
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                if (modbus == null)
                    throw new Exception("Modbus have yet to be initialized");
                return await modbus.ReadHoldingRegistersAsync(client_id, address, length);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + " | " + e.StackTrace);
                return [];
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public async Task<bool> WriteData(ushort address, ushort value , byte client_id = 1)
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                if (modbus == null)
                    throw new Exception("Modbus have yet to be initialized");
                await modbus.WriteSingleRegisterAsync(client_id, address, value);
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + " | " + e.StackTrace);
                return false;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

    }
}
