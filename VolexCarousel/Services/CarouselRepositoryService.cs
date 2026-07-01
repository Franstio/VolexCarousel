using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VolexCarousel.Models;

namespace VolexCarousel.Services
{
    public class CarouselRepositoryService
    {
        public readonly string db_scheme = @"CREATE TABLE IF NOT EXISTS ""tbl_shift"" (
	""shiftname""	TEXT,
	""targetoutput""	INTEGER NOT NULL,
    ""shiftstart"" TEXT NOT NULL,
    ""shiftend"" TEXT NOT NULL,  
	PRIMARY KEY(""shiftname"")
);
CREATE TABLE IF NOT EXISTS ""tbl_shiftrecord"" (
	""id""	INTEGER NOT NULL,
	""shiftname""	TEXT NOT NULL,
	""datetimeinput""	TEXT NOT NULL,
	""datetimeoutput""	TEXT NOT NULL,
    ""targetoutput"" INTEGER NOT NULL,
	PRIMARY KEY(""id"" AUTOINCREMENT)
);
CREATE TABLE IF NOT EXISTS ""tbl_users"" (
	""username""	TEXT NOT NULL,
	""password""	TEXT NOT NULL,
	PRIMARY KEY(""username"")
);
";
        private readonly ILogger<CarouselRepositoryService> logger;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly IDbConnection db;

        public CarouselRepositoryService(ILogger<CarouselRepositoryService> logger, IDbConnection db)
        {
            this.logger = logger;
            this.db = db;
        }
        public async Task Initialization()
        {
            using (db)
            {
                using (db)
                {
                    IDbTransaction tr = null!;
                    try
                    {
                        await semaphore.WaitAsync();
                        db.Open();
                        tr = db.BeginTransaction(IsolationLevel.Serializable);
                        await tr.Connection!.ExecuteAsync(db_scheme);
                        tr.Commit();
                        var usr = await GetUser();
                        if (!usr.Any())
                        {
                            await db.ExecuteAsync("insert into tbl_users(username,password) values('admin','123')");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e.Message + " | " + e.StackTrace);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
            }
        }

        public async Task<IEnumerable<User>> GetUser(string? username = null)
        {
            using (db)
            {
                try
                {
                    db.Open();
                    return await db.QueryAsync<User>($"Select username,password from tbl_users where username=@username or @username is null", new { username });
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    return [];
                }
            }
        }

        public async Task RecordItemInput(ShiftTransactionRecord record)
        {
            using (db)
            {
                IDbTransaction tr = null!;
                try
                {
                    await semaphore.WaitAsync();
                    db.Open();
                    tr = db.BeginTransaction(IsolationLevel.Serializable);
                    await tr.Connection!.ExecuteAsync("Insert into tbl_shiftrecord(shiftname,datetimeinput,datetimeoutput) values(@shiftname,@datetimeinput,@datetimeoutput);", record);
                    tr.Commit();
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    tr.Rollback();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        public async Task<IEnumerable<ShiftTransactionRecord>> GetRecordItems(int limit = 100)
        {
            using (db)
            {
                try
                {
                    db.Open();
                    var query = await db.QueryAsync<ShiftTransactionRecord>($"Select shiftname,datetimeinput,datetimeoutput,targetoutput from tbl_shiftrecord order by datetimeoutput desc limit @limit", limit);
                    return query;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    return [];
                }
            }
        }
        public async Task<IEnumerable<ShiftTransactionRecord>> GetTodayShiftRecord(string shift, int limit = 100)
        {
            using (db)
            {
                try
                {
                    db.Open();
                    var query = await db.QueryAsync<ShiftTransactionRecord>($"Select r.shiftname,datetimeinput,datetimeoutput,targetoutput from tbl_shiftrecord r where Date(r.datetimeoutput)=Date('now') and r.shiftname=@shift order by datetimeinput limit @limit", new { shift, limit });
                    return query;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    return [];
                }
            }
        }
        public async Task<IEnumerable<ShiftRecordRowModel>> GetTodayShiftDisplay(string shift, int limit = 100)
        {
            try
            {
                var query = await GetTodayShiftRecord(shift, limit);
                using (db)
                {
                    db.Open();
                    var data = query.GroupBy(x => new { shiftname = x.shiftname, hour = x.datetimeoutput.Hour }).SelectMany(x => x.Select(z => new ShiftRecordRowModel()
                    {
                        Timestamp = z.datetimeoutput,
                        TargetOutput = z.targetoutput,
                        Output = x.Count(),
                    }));
                    return data!;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message + " | " + e.StackTrace);
                return [];
            }
        }
        public async Task<IEnumerable<ShiftDailyOutputModel>> GetDailyOutput()
        {
            using (db)
            {
                try
                {
                    db.Open();
                    var query = await db.QueryAsync<ShiftTransactionRecord>("Select r.shiftname,datetimeinput,datetimeoutput,targetoutput from tbl_shift s left join tbl_shiftrecord r  on s.shiftname=r.shiftname and DAte(r.datetimeoutput)=DATE('now') and r.shiftname=@shift");
                    var data = query.GroupBy(x => x.shiftname).SelectMany(x => x.Select(z => new ShiftDailyOutputModel()
                    {
                        ShiftName = x.Key,
                        TargetOutput = z.targetoutput,
                        TotalOutput = x.Count()
                    }));
                    return data!;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    return [];
                }
            }
        }

        public async Task UpdateTargetOutput(int targetOutput, string? shift = null)
        {
            using (db)
            {
                IDbTransaction tr = null!;
                try
                {
                    await semaphore.WaitAsync();
                    db.Open();
                    tr = db.BeginTransaction(IsolationLevel.Serializable);
                    await tr.Connection!.ExecuteAsync("Update tbl_shift set targetoutput=@targetoutput where shiftname=@shiftname or @shiftname is null", new { shiftname = shift, targetoutput = targetOutput });
                    tr.Commit();
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    tr.Rollback();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }
        public async Task<IEnumerable< ShiftMasterRecord>> GetShift(string? shift = null)
        {
            using (db)
            {
                try
                {
                    db.Open();
                    var query = await db.QueryAsync<ShiftMasterRecord>("Select shiftname,targetoutput,shiftstart,shiftend from tbl_shift where shiftname=@shift or @shift is null", new { shift });
                    return query;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    return [];
                }
            }
        }

        public async Task AddShift(ShiftMasterRecord shift)
        {
            using (db)
            {
                IDbTransaction tr = null!;
                try
                {
                    await semaphore.WaitAsync();
                    db.Open();
                    tr = db.BeginTransaction(IsolationLevel.Serializable);
                    await tr.Connection!.ExecuteAsync("Insert into tbl_shift(shiftname,targetoutput,shiftstart,shiftend) values(@shiftname,@targetoutput,@shiftstart,@shiftend);", new { shiftname = shift.shiftname, targetoutput = shift.targetoutput, shiftstart = shift.shiftstart.ToString("HH:mm:ss"), shiftend = shift.shiftend.ToString("HH:mm:ss") });
                    tr.Commit();
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    tr.Rollback();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        public async Task UpdateShiftMaster(string shift, ShiftMasterRecord shiftData)
        {
            using (db)
            {
                IDbTransaction tr = null!;
                try
                {
                    await semaphore.WaitAsync();
                    db.Open();
                    tr = db.BeginTransaction(IsolationLevel.Serializable);
                    await tr.Connection!.ExecuteAsync("Update tbl_shift set targetoutput=@targetoutput,shiftstart=@shiftstart,shiftend=@shiftend where shiftname=@shiftname", new { shiftname = shift, targetoutput = shiftData.targetoutput, shiftstart = shiftData.shiftstart.ToString(@"hh:mm:ss"), shiftend =shiftData.shiftend.ToString(@"hh:mm:ss") });
                    tr.Commit();
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + " | " + e.StackTrace);
                    tr.Rollback();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }
    }
}