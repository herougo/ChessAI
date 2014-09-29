using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessProgram
{
    class OpeningBook
    {
        public int[,] White_Piece_Count { get; set; }
        public string[,] White_Board { get; set; }
        public string[,] White_Move { get; set; }
        // nth entry corresponds to the 2n+1 move
        // ith move corresponds to the i / 2 (or (i - 1) / 2) entry
        // ie 0 -> move 1, 1 -> move 3
        public int white_moves_depth { get; set; } // first entry length
        public int white_moves_width { get; set; } // second entry length

        public int[,] Black_Piece_Count { get; set; }
        public string[,] Black_Board { get; set; }
        public string[,] Black_Move { get; set; }
        // nth entry corresponds to the 2n+2 move
        // ith move corresponds to the i / 2 - 1 (or (i - 1) / 2) entry
        // ie 0 -> move 2, 1 -> move 4
        public int black_moves_depth { get; set; } // first entry length
        public int black_moves_width { get; set; } // second entry length

        public OpeningBook(string[] line_names)
        {
            if (line_names == null)
            {
                White_Piece_Count = null;
                White_Board = null;
                White_Move = null; 
                white_moves_depth = 0;
                white_moves_width = 0;

                Black_Piece_Count = null;
                Black_Board = null;
                Black_Move = null;
                black_moves_depth = 0;
                black_moves_width = 0;
            }
            else
            {
                white_moves_depth = 0;
                white_moves_width = 0;
                black_moves_depth = 0;
                black_moves_width = 0;
                int counter = 0;

                string[,] temp_white_moves = new string[100, 100];
                string[,] temp_black_moves = new string[100, 100];

                #region get dimensions 
                // ** inefficient?**
                for (int i = 0; i < line_names.Length; i++)
                {
                    Database db = new Database("Opening Book - " + line_names[i]);
                    int length = db.entry_length;
                    int move_entry;

                    for (int j = 0; j < length; j++)
                    {
                        move_entry = (Convert.ToInt32(db.database_array[j, 1]) - 1) / 2;

                        if (db.database_array[j, 0] == "W")
                        {
                            if (move_entry >= white_moves_depth) white_moves_depth = move_entry + 1;

                            counter = 0;
                            while (temp_white_moves[move_entry, counter] != null)
                            {
                                counter++;
                            }
                            temp_white_moves[move_entry, counter] = "";

                            if (counter >= white_moves_width) white_moves_width = counter + 1;
                        }
                        else
                        {
                            if (move_entry >= black_moves_depth) black_moves_depth = move_entry + 1;

                            counter = 0;
                            while (temp_black_moves[move_entry, counter] != null)
                            {
                                counter++;
                            }
                            temp_black_moves[move_entry, counter] = "";

                            if (counter >= black_moves_width) black_moves_width = counter + 1;
                        }
                    }
                }
                #endregion

                White_Piece_Count = new int[white_moves_depth, white_moves_width];
                White_Board = new string[white_moves_depth, white_moves_width];
                White_Move = new string[white_moves_depth, white_moves_width];

                Black_Piece_Count = new int[black_moves_depth, black_moves_width];
                Black_Board = new string[black_moves_depth, black_moves_width];
                Black_Move = new string[black_moves_depth, black_moves_width];

                for (int i = 0; i < line_names.Length; i++)
                {
                    Database db = new Database("Opening Book - " + line_names[i]);
                    int length = db.entry_length;
                    int move_entry;

                    for (int j = 0; j < length; j++)
                    {
                        move_entry = (Convert.ToInt32(db.database_array[j, 1]) - 1) / 2;

                        if (db.database_array[j, 0] == "W")
                        {
                            counter = 0;
                            while (White_Board[move_entry, counter] != null)
                            {
                                counter++;
                            }
                            White_Piece_Count[move_entry, counter] 
                                = Convert.ToInt32(db.database_array[j, 2]);
                            White_Board[move_entry, counter] = db.database_array[j, 3];
                            White_Move[move_entry, counter] = db.database_array[j, 4];
                        }
                        else
                        {
                            counter = 0;
                            while (Black_Board[move_entry, counter] != null)
                            {
                                counter++;
                            }
                            Black_Piece_Count[move_entry, counter]
                                = Convert.ToInt32(db.database_array[j, 2]);
                            Black_Board[move_entry, counter] = db.database_array[j, 3];
                            Black_Move[move_entry, counter] = db.database_array[j, 4];
                        }
                    }
                }
            }
        }


    }
}
