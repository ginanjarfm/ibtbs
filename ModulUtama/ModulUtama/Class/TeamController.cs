﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TubesAI.Model;

namespace ModulUtama.Class
{
    /*
       Jenis2 Set image:
        AC.Attack(ElemenAksi A,int poin,bool miss);
        AC.Kill(ElemenAksi A,int poin);
        AC.Defend(ElemenAksi A);
        AC.Heal(ElemenAksi A,int poin,int miss);
        AC.UseItem(ElemenAksi A,int poin,int miss);
        AC.Win(int Team);
    */

    /// <summary>
    /// Controller yang mengatur giliran unit untuk melakukan aksi,
    /// Kalkulasi setiap aksi, dan menyuruh Animation Controller untuk menggambar tampilan game
    /// </summary>
    class TeamController
    {
        #region properties
        /// <summary>
        /// Animation Controller
        /// </summary>
        private AnimationController AC;
        /// <summary>
        /// Team 1
        /// </summary>
        public Team Team1 {get; private set;}
        /// <summary>
        /// Team 2
        /// </summary>
        public Team Team2 { get; private set; }
        /// <summary>
        /// Jika bernilai true maka Team 1 menyerang dahulu, 
        /// jika bernilai false maka Team 2 menyerang dahulu.
        /// </summary>
        private bool FirstMove;
        /// <summary>
        /// Damage normal = 200 poin
        /// </summary>
        private const int Damage = 200;
        /// <summary>
        /// Heal normal = 500 poin
        /// </summary>
        private const int Heal = 500;
        /// <summary>
        /// Jumlah item normal = 10 item
        /// </summary>
        private const int numitem = 10;
        #endregion

        #region constructors

        /// <param name="te1">Team 1</param>
        /// <param name="te2">Team 2</param>
        /// <param name="firstMove">
        /// boolean yang menentukan Team mana yang jalan duluan
        /// jika bernilai true, Team 1 menyerang dahulu,
        /// jika bernilai false, Team 2 menyerang dahulu.
        /// </param>
        public TeamController(Team te1,Team te2,bool firstMove)
        {
            //Inisiaslisasi Team
            Team1 = te1;
            Team2 = te2;
            
            // Inisialisasi Animation Controller
            AC = new AnimationController(Team1, Team2);

            //Inisialisasi atribut lainnya
            FirstMove = firstMove;
        }
        #endregion

        #region methods

        private Unit FindUnit(int index)
        {
            if (index < 11)
            {
                return Team1.FindUnit(index);
            }
            else
            {
                return Team2.FindUnit(index - 11);
            }
        }

        /// <summary>
        /// Memberikan ElemenAksi pada listelemenaksi pada index ke-index
        /// </summary>
        /// <param name="listelemenaksi">list of ElemenAksi</param>
        /// <param name="index">integer index</param>
        /// <returns></returns>
        private static ElemenAksi FindElemenAksi(List<ElemenAksi> listelemenaksi,int index)
        {
            foreach (ElemenAksi el in listelemenaksi)
            {
                if (listelemenaksi.FindIndex(re => re == el) == index) return el;
            }
            return null;
        }


        /// <summary>
        /// Me-restore semua Unit pada masing-masing Team.
        /// Membekali masing-masing Team dengan 10 potion dan 10 life Potion.
        /// </summary>
        public void ResetTeam()
        {
            // Fullkan HP seluruh unit pada Team 1 dan Team 2
            foreach (Unit un in Team1.listUnit)
            {
                un.setHP(un.getMaxHP());
            }
            foreach (Unit un in Team2.listUnit)
            {
                un.setHP(un.getMaxHP());
            }
            // beri 10 potion dan 10 life potion
            Team1.giveLifePotion(numitem);
            Team1.givePotion(numitem);
            Team2.giveLifePotion(numitem);
            Team2.givePotion(numitem);
        }
        
        /// <summary>
        /// Mengembalikan true jika ada team yang menang.
        /// </summary>
        /// <returns>boolean</returns>
        public bool isEndGame()
        {
            // Cek apakah ada team yang menang 
            int count1 = 0;
            int count2 = 0;
            foreach (Unit un in Team1.listUnit)
            {
                if (un.isDead())
                {
                    count1++;
                }
            }

            foreach (Unit un in Team2.listUnit)
            {
                if (un.isDead())
                {
                    count2++;
                }
            }
            
            return (count1 == 11 || count2 == 11);
        }


        /// <summary>
        /// Kalkulasi damage yang dihasilkan dari Unit satu kepada Unit dua
        /// </summary>
        /// <param name="satu">Unit yang menyerang</param>
        /// <param name="dua">Unit yang diserang</param>
        public int CalculationDamage(Unit satu,Unit dua)
        {
            int DamageTaken = Damage;

            if (satu is Archer)
            {
                if (dua is Rider)//kelemahan
                    DamageTaken /= 2;
                else if ((dua is Swordsman) || (dua is Medic)) //kuat
                    DamageTaken = (int)(DamageTaken * 1.5);
            }
            else if (satu is Swordsman)
            {
                if (dua is Archer)//kelemahan
                    DamageTaken /= 2;
                else if ((dua is Spearman) || (dua is Medic)) //kuat
                    DamageTaken = (int)(DamageTaken * 1.5);
            }
            else if (satu is Spearman)
            {
                if (dua is Swordsman)//kelemahan
                    DamageTaken /= 2;
                else if ((dua is Rider) || (dua is Medic)) //kuat
                    DamageTaken = (int)(DamageTaken * 1.5);
            }
            else if (satu is Rider)
            {
                if (dua is Spearman)//kelemahan
                    DamageTaken /= 2;
                else if ((dua is Archer) || (dua is Medic)) //kuat
                    DamageTaken = (int)(DamageTaken * 1.5);
            }
            if (dua.isBertahan) DamageTaken /= 2;
            dua.setHP(dua.getCurrentHP() - DamageTaken);

            return DamageTaken;
        }
        
        /// <summary>
        /// Kalkulasi Heal dari Unit satu kepada Unit dua
        /// </summary>
        /// <param name="satu">Unit yang heal</param>
        /// <param name="dua">Unit yang diheal</param>
        public void CalculationHeal(Unit satu,Unit dua)
        {
            dua.setHP(dua.getCurrentHP() + Heal);
            if (dua.getCurrentHP() > dua.getMaxHP()) dua.setHP(dua.getMaxHP());
        }
        
        /// <summary>
        /// Kalkulasi Life potion yang diberikan dari Unit satu kepada Unit dua
        /// </summary>
        /// <param name="satu">Unit yang memberi life potion</param>
        /// <param name="dua">Unit yang diberi life potion</param>
        public void CalculationLife(Unit satu,Unit dua)
        {
            dua.setHP((int)(0.5 * dua.getMaxHP()));
        }

        private void AddAction(Unit attacker, Unit defender, ElemenAksi action, int team)
        {
            int _subject = attacker.index + (team * 11);
            int _object = defender.index + ((1 - action.tim_sasaran) % 2 * 11);
            switch (action.aksi)
            {
                case Aksi.menyerang:
                    {
                        //Aksi dijalankan!
                        //unit yang diserang belum mati
                        if (!defender.isDead())
                        {
                            int Damage = CalculationDamage(attacker, defender);
                            AC.Attack(_subject, _object, Damage, false);
                        }
                        //unit sudah mati
                        else
                        {
                            AC.Attack(_subject, _object, 0, true);
                        }
                        break;
                    }
                case Aksi.heal:
                    {
                        //Aksi dijalankan!
                        //unit yang disembuhkan belum mati
                        if (!defender.isDead())
                        {
                            CalculationHeal(attacker, defender);
                            AC.Heal(_subject, _object, Heal, false);
                        }
                        //unit sudah mati
                        else
                        {
                            AC.Heal(_subject, _object, 0, true);
                        }
                        break;
                    }
                case Aksi.use_item:
                    {
                        switch (action.item)
                        {
                            case Item.potion:
                                {
                                    if (!defender.isDead())
                                    {
                                        CalculationHeal(attacker, defender);
                                        AC.UseItem(_subject, _object, Item.potion, Heal, false);
                                    }
                                    else
                                    {
                                        AC.UseItem(_subject, _object, Item.potion, 0, true);
                                    }
                                    break;
                                }
                            case Item.life_potion:
                                {
                                    if (!defender.isDead())
                                    {
                                        CalculationLife(attacker, defender);
                                        AC.UseItem(_subject, _object, Item.life_potion, (defender.getMaxHP() / 2), false);
                                    }
                                    else
                                    {
                                        AC.UseItem(_subject, _object, Item.life_potion, 0, true);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Mengatur giliran Unit mana dulu yang berjalan dahulu
        /// </summary>
        /// <param name="actsTeam1">List aksi Team 1</param>
        /// <param name="actsTeam2">List aksi Team 2</param>
        public void AturGiliran(List<ElemenAksi> actsTeam1, List<ElemenAksi> actsTeam2)
        {
            /*************************************************/
            /*************************************************/

            // Animasi unit yang mati
            foreach (var unit in Team1.listUnit)
            {
                if (unit.isDead())
                {
                    AC.Dead(unit.index);
                }
            }
            foreach (var unit in Team2.listUnit)
            {
                if (unit.isDead())
                {
                    AC.Dead(unit.index + 11);
                }
            }

            // Animasi unit yang bertahan
            foreach (var action in actsTeam1)
            {
                if (action.aksi == Aksi.bertahan)
                {
                    AC.Defend(action.index_pelaku);
                }
            }
            foreach (var action in actsTeam2)
            {
                if (action.aksi == Aksi.bertahan)
                {
                    AC.Defend(action.index_pelaku + 11);
                }
            }

            // Pilih Unit yang akan Pilih
            // Setelah tidak ada unit yang bertahan yang dapat dipilih, mulai pilih dari yang tercepat hingga terlambat
            // Setiap pemilihan unit, cek apakah unit masih hidup
            /*************************************************/

            #region Masukkan Archer

            // TEAM1
            if (FirstMove == true)
            {
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);
                        
                    if (attacker is Archer && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Archer && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }
            // TEAM2
            else
            {
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Archer  && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Archer  && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }

            #endregion

            #region Masukkan Swordsman

            // TEAM1
            if (FirstMove == true)
            {
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Swordsman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Swordsman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }
            // TEAM2
            else
            {
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Swordsman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Swordsman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }

            #endregion

            #region Masukkan Spearman

            // TEAM1
            if (FirstMove == true)
            {
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Spearman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Spearman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }
            // TEAM2
            else
            {
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Spearman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Spearman && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }

            #endregion

            #region Masukkan Medic

            // TEAM1
            if (FirstMove == true)
            {
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Medic && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Medic && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }
            // TEAM2
            else
            {
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Medic && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Medic && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }

            #endregion

            #region Masukkan Rider

            // TEAM1
            if (FirstMove == true)
            {
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Rider && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Rider && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }
            // TEAM2
            else
            {
                foreach (var action in actsTeam2)
                {
                    Unit attacker = Team2.FindUnit(action.index_pelaku);

                    if (attacker is Rider && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 0);
                    }
                }
                foreach (var action in actsTeam1)
                {
                    Unit attacker = Team1.FindUnit(action.index_pelaku);

                    if (attacker is Rider && !attacker.isDead())
                    {
                        Unit defender = FindUnit(action.tim_sasaran * 11 + action.index_sasaran);
                        AddAction(attacker, defender, action, 1);
                    }
                }
            }

            #endregion

            // Jalankan unit yang dipilih:
            //  Jika unit attack,
            //      Jika unit yang diattack belum mati
            //          Panggil Calculation
            //          Set image untuk kasus ini
            //      Jika unit yang diattack sudah mati
            //          Set image untuk kasus ini
            //  Jika unit bertahan,
            //		Set image untuk kasus ini
            //  Jika unit heal,
            //      Jika unit yang diheal belum mati
            //          Tambahkan HP pada unit yang diheal
            //          Set image untuk kasus ini
            //      Jika unit yang diheal sudah mati
            //          Set image untuk kasus ini
            //  Jika unit pake potion,
            //		Jika potion masih ada, 
            //      	Jika unit yang dikasih potion belum mati
            //          	Tambahkan HP pada unit yang dikasih potion
            //          	Kurangi jumlah potion pada tim
            //          	Set image untuk kasus ini
            //      	Jika unit yang dikasih potion sudah mati
            //          	Kurangi jumlah potion pada tim ?? Kesepakatan ??
            //          	Set image untuk kasus ini
            //		Jika potion tidak ada,
            //			Set image untuk kasus ini
            // Jika unit pake life potion,
            //		Jika life potion masih ada,
            //      	Jika unit yang dikasih life potion belum mati
            //          	Kurangi jumlah life potion pada tim ?? Kesepakatan ??
            //          	Set image untuk kasus ini
            //      	Jika unit yang dikasih life potion sudah mati
            //          	Tambahkan 50% dari max HP pada unit yang dikasih life potion 
            //          	Kurangi jumlah life potion pada tim
            //          	Set image untuk kasus ini
            //		Jika life potion tidak ada,
            //			Set image untuk kasus ini
            // Jika do nothing,
            //		Set image untuk kasus ini

        }

        #endregion

    }
}