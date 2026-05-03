using Fluence.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluence.Services
{
    class WalletService : BaseDatabaseService
    {
        public async Task<List<Wallet>> GetWalletsAsync()
        {
            var db = await GetDbAsync();
            var wallets = await db.Table<Wallet>().ToListAsync();
            
            if (wallets.Count == 0)
            {
                var profileService = new ProfileService();
                var profile = await profileService.GetProfileAsync();
                double initialBalance = profile?.InitialBalance ?? 0;

                var defaultWallet = new Wallet { Name = "cash", Balance = initialBalance };
                await AddWalletAsync(defaultWallet);
                wallets = await db.Table<Wallet>().ToListAsync();
            }
            
            return wallets;
        }

        public async Task<Wallet> GetWalletByIdAsync(int id)
        {
            if (id <= 0) return null;
            var db = await GetDbAsync();
            return await db.Table<Wallet>().Where(w => w.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddWalletAsync(Wallet wallet)
        {
            if (wallet == null) return;
            if (wallet.Name != null) wallet.Name = wallet.Name.ToLower();
            var db = await GetDbAsync();
            await db.InsertAsync(wallet);
        }

        public async Task UpdateWalletAsync(Wallet wallet)
        {
            if (wallet == null) return;
            if (wallet.Name != null) wallet.Name = wallet.Name.ToLower();
            var db = await GetDbAsync();
            await db.UpdateAsync(wallet);
        }

        public async Task DeleteWalletAsync(int id)
        {
            var db = await GetDbAsync();
            var wallet = await GetWalletByIdAsync(id);
            if (wallet != null && wallet.Name != "cash")
            {
                await db.DeleteAsync(wallet);
            }
        }

        public async Task UpdateBalanceAsync(int walletId, double amount, string type, bool isAddition = true)
        {
            var wallet = await GetWalletByIdAsync(walletId);
            if (wallet == null) return;

            double adjustment = type == "Income" ? amount : -amount;
            if (!isAddition) adjustment = -adjustment; 

            wallet.Balance += adjustment;
            await UpdateWalletAsync(wallet);
        }
    }
}
