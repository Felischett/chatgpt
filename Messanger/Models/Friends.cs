using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Friends
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public int FriendUserId {get; set;}
        public bool Angenommen {get; set;}
    }
}
