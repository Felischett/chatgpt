using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ORM.Services
{
    public class DbManager : DbContext
    {
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Friends> Friends { get; set; }
        public DbSet<Messages> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=localhost;database=Messanger;user=root;password=root123456";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        public async Task<User?> findeBenutzerNachIdAsync(int id)
            => await Users.FindAsync(id);

        public async Task<User?> findeBenutzerNachNameAsync(string username)
            => await Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<User?> findeBenutzerNachEmailAsync(string email)
            => await Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> findeBenutzerNachSchluesselAsync(string key)
            => await Users.FirstOrDefaultAsync(u => u.Key == key);

        public async Task<User> registriereBenutzerAsync(User benutzer)
        {
            benutzer.Id = 0;
            await Users.AddAsync(benutzer);
            await SaveChangesAsync();
            return benutzer;
        }

        public async Task<bool> saveToDbAsync()
        {
            try { return await SaveChangesAsync() >= 1; }
            catch { }
            return false;
        }

        public async Task<List<User>> holeAnfragenAsync(int userId)
        {
            var senderIds = await Friends
                .Where(f => f.Angenommen == false && f.FriendUserId == userId)
                .Select(f => f.UserId)
                .Distinct()
                .ToListAsync();

            return await Users.Where(u => senderIds.Contains(u.Id)).ToListAsync();
        }

        public async Task<List<User>> holeFreundeAsync(int userId)
        {
            var friendPairs = await Friends
                .Where(f => f.Angenommen == true && (f.UserId == userId || f.FriendUserId == userId))
                .Select(f => new { f.UserId, f.FriendUserId })
                .ToListAsync();

            var friendIds = friendPairs
                .Select(p => p.UserId == userId ? p.FriendUserId : p.UserId)
                .Distinct()
                .ToList();

            return await Users.Where(u => friendIds.Contains(u.Id)).ToListAsync();
        }

        public async Task sendeFreundschaftsanfrageAsync(int userId, string friendKey)
        {
            var friendUser = await Users.FirstOrDefaultAsync(u => u.Key == friendKey);
            if (friendUser == null) throw new Exception("Kein Benutzer mit diesem Key gefunden.");
            if (friendUser.Id == userId) throw new Exception("Du kannst dich nicht selbst hinzufügen.");

            var existiertSchon = await Friends.AnyAsync(f =>
                (f.UserId == userId && f.FriendUserId == friendUser.Id) ||
                (f.UserId == friendUser.Id && f.FriendUserId == userId));

            if (existiertSchon) throw new Exception("Freundschaft oder Anfrage existiert bereits.");

            await Friends.AddAsync(new Friends
            {
                UserId = userId,
                FriendUserId = friendUser.Id,
                Angenommen = false
            });

            await SaveChangesAsync();
        }

        public async Task bestaetigeFreundschaftAsync(int userId, int friendUserId)
        {
            var anfrage = await Friends.FirstOrDefaultAsync(f =>
                f.Angenommen == false && f.UserId == friendUserId && f.FriendUserId == userId);

            if (anfrage == null)
            {
                anfrage = await Friends.FirstOrDefaultAsync(f =>
                    f.Angenommen == false && f.UserId == userId && f.FriendUserId == friendUserId);
            }

            if (anfrage == null) throw new Exception("Keine passende Anfrage gefunden.");

            anfrage.Angenommen = true;
            await SaveChangesAsync();
        }

        public async Task lehneFreundschaftAsync(int userId, int friendUserId)
        {
            var anfrage = await Friends.FirstOrDefaultAsync(f =>
                f.Angenommen == false && f.UserId == friendUserId && f.FriendUserId == userId);

            if (anfrage == null)
            {
                anfrage = await Friends.FirstOrDefaultAsync(f =>
                    f.Angenommen == false && f.UserId == userId && f.FriendUserId == friendUserId);
            }

            if (anfrage == null) throw new Exception("Keine passende Anfrage gefunden.");

            Friends.Remove(anfrage);
            await SaveChangesAsync();
        }

        public async Task<List<Messages>> holeChatAsync(int userId, int friendUserId, int take = 200)
        {
            return await Messages
                .Where(m =>
                    (m.SenderId == userId && m.EmpfaengerId == friendUserId) ||
                    (m.SenderId == friendUserId && m.EmpfaengerId == userId))
                .OrderByDescending(m => m.SentAt)
                .Take(take)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        // WICHTIG: gibt jetzt die gespeicherte Message zurück
        public async Task<Messages> sendeMessageAsync(int senderId, int empfaengerId, string encryptedMessage)
        {
            var msg = new Messages
            {
                SenderId = senderId,
                EmpfaengerId = empfaengerId,
                Message = encryptedMessage,
                SentAt = DateTime.Now
            };

            await Messages.AddAsync(msg);
            await SaveChangesAsync();

            return msg;
        }

        public async Task<UserSettings> holeUserSettingsAsync(int userId)
        {
            var settings = await UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings { UserId = userId, TargetLang = "DE" };
                await UserSettings.AddAsync(settings);
                await SaveChangesAsync();
            }

            return settings;
        }

        public async Task<UserSettings> speichereUserSettingsAsync(int userId, string targetLang)
        {
            targetLang = (targetLang ?? "DE").Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(targetLang)) targetLang = "DE";

            var settings = await UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings { UserId = userId, TargetLang = targetLang };
                await UserSettings.AddAsync(settings);
            }
            else
            {
                settings.TargetLang = targetLang;
                UserSettings.Update(settings);
            }

            await SaveChangesAsync();
            return settings;
        }
    }
}
