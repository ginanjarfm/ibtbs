﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace TubesAI.Model
{
    public class Spearman : Unit
    {
        public static Texture2D texture;

        public Spearman(int _index)
        {
            //assignment atribut yang dasar di kelas ini.
            index = _index;
            this.maxHP = 2000;
            this.currentHP = this.maxHP;
            this.urutan = 3;
            isBertahan = false;
        }
    }
}