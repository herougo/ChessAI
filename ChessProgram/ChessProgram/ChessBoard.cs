using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessProgram
{
    class ChessBoard
    {
        // B B B B B B B B
        // B B B B B B B B
        // - - - - - - - -
        // - - - - - - - -
        // - - - - - - - -
        // - - - - - - - -
        // W W W W W W W W
        // W W W W W W W W

        /*
        (0, 0) (0, 1) (0, 2) ...
        (1, 0) (1, 1) (1, 2) ...
        .
        .
        .

        */

        public string[] Possible_Moves = null;
        public string[,] Position { get; set; }
        public string Turn { get; set; }

        public int fast_pawn_column { get; set; }
        public bool left_white_castle_moved { get; set; }
        public bool right_white_castle_moved { get; set; }
        public bool left_black_castle_moved { get; set; }
        public bool right_black_castle_moved { get; set; }

        public int piece_count { get; set; }
        
        private string[] promotion_options = { "Q", "R", "B", "N" };

        public ChessBoard(string[,] new_board, string colour_turn, int fast_pawn_col, bool left_white_castle,
            bool right_white_castle, bool left_black_castle, bool right_black_castle, int start_piece_count)
        {
            Position = new_board;
            Turn = colour_turn;

            // Extra Variables
            fast_pawn_column = fast_pawn_col;
            left_white_castle_moved = left_white_castle;
            right_white_castle_moved = right_white_castle;
            left_black_castle_moved = left_black_castle;
            right_black_castle_moved = right_black_castle;

            piece_count = start_piece_count;

            UpdatePossibleMoves();
        }

        public ChessBoard BoardCopy()
        {
            ChessBoard result = new ChessBoard(PosCopy(), Turn, fast_pawn_column, 
                left_white_castle_moved, right_white_castle_moved, left_black_castle_moved, 
                right_black_castle_moved, piece_count);

            return result;
        }

        public string[,]  PosCopy()
        {
            string[,] result = new string[8, 8];

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    result[r, c] = this.Position[r, c];
                }
            }

            return result;
        }

        public int MoveToInt(string move)
        {
            int length = Possible_Moves.Length;

            if (move == null)
            {
                return -1;
            }

            for (int i = 0; i < length; i++)
            {
                if (Possible_Moves[i] == move)
                {
                    return i;
                }
            }

            return -1;
        }

        public string BoardToString()
        {
            string result = "";

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (Position[r, c] != "")
                    {
                        result += "(" + r.ToString() + " " + c.ToString() + ") "
                            + Position[r, c] + "\n";
                    }
                }
            }

            return result.Trim();
        }

        public bool PossibleMove(string move)
        {
            bool result = false;

            for (int i = 0; i < Possible_Moves.Length; i++)
            {
                if (Possible_Moves[i].Substring(0, 14) == move) result = true;
            }

            return result;
        }

        public void MakeMove(string move)
        {
            // move format: (1 2) -> (1 2)
            //              12345678901234
            // but may an " -> Q" to represent promoting a piece

            if (move == null)
            {
                return;
            }

            // virtual move
            int r1 = Convert.ToInt32(move.Substring(1, 1));
            int r2 = Convert.ToInt32(move.Substring(10, 1));
            int c1 = Convert.ToInt32(move.Substring(3, 1));
            int c2 = Convert.ToInt32(move.Substring(12, 1));

            // update piece_count
            if (Position[r2, c2] != "")
            {
                piece_count--;
            }

            Position[r2, c2] = Position[r1, c1];
            Position[r1, c1] = "";

            // check promotion
            if (move.Length > 14)
            {
                Position[r2, c2] = Position[r2, c2].Substring(0, 6)
                    + move.Substring(18, 1);
            }

            // en passant
            if (Position[r2, c2].Substring(6) == "P"
                && (r1 - r2 == -2 || r1 - r2 == 2))
            {
                fast_pawn_column = c1;
            }
            else if (fast_pawn_column == c2
                && (r2 == 2 && Turn == "W" ||
                    r2 == 5 && Turn == "B")
                && Position[r2, c2].Substring(6) == "P")
            {
                Position[r1, c2] = "";
                piece_count--;
                fast_pawn_column = -1;
            }
            else
            {
                fast_pawn_column = -1;
            }


            // move rook for castle
            if (Position[r2, c2].Length > 0 && Position[r2, c2].Substring(6, 1) == "K"
                && c2 - c1 == 2)
            {
                Position[r2, c2 - 1] = Position[r2, 7];
                Position[r2, 7] = "";
            }
            if (Position[r2, c2].Length > 0 && Position[r2, c2].Substring(6, 1) == "K"
                && c2 - c1 == -2)
            {
                Position[r2, c2 + 1] = Position[r2, 0];
                Position[r2, 0] = "";
            }

            // check castle moved
            if (Position[0, 0] != "Black R" || Position[0, 4] != "Black K")
                left_black_castle_moved = true;
            if (Position[7, 0] != "White R" || Position[7, 4] != "White K")
                left_white_castle_moved = true;
            if (Position[0, 7] != "Black R" || Position[0, 4] != "Black K")
                right_black_castle_moved = true;
            if (Position[7, 7] != "White R" || Position[7, 4] != "White K")
                right_white_castle_moved = true;

            ChangeTurn();

            UpdatePossibleMoves();
        }

        public void ChangeTurn()
        {
            Turn = OtherTurn(Turn);
        }

        public string OtherTurn(string turn)
        {
            if (turn == "W") return "B";
            else { return "W"; }
        }

        public bool CheckVirtualCheck(string move)
        {
            // checks if a move puts the turn player in check
            
            bool result = false;

            string[,] virtual_board = new string[8, 8];

            // copy vitrual_board
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    virtual_board[r, c] = Position[r, c];
                }
            }

            if (move != null)
            {
                // move format: (1 2) -> (1 2)
                //              12345678901234
                // but may an " -> Q" to represent promoting a piece

                // virtual move
                int r1 = Convert.ToInt32(move.Substring(1, 1));
                int r2 = Convert.ToInt32(move.Substring(10, 1));
                int c1 = Convert.ToInt32(move.Substring(3, 1));
                int c2 = Convert.ToInt32(move.Substring(12, 1));

                virtual_board[r2, c2] = virtual_board[r1, c1];
                virtual_board[r1, c1] = "";

                // check promotion
                if (move.Length > 14)
                {
                    virtual_board[r2, c2] = virtual_board[r2, c2].Substring(0, 6)
                        + move.Substring(18, 1);
                }
            }

            string entry;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    // Find King
                    entry = virtual_board[r, c];
                    if (entry.Length > 0
                        && entry.Substring(0, 1) == Turn
                        && entry.Substring(6) == "K")
                    {
                        int counter;
                        int counter2;

                        #region Find R or Q
                        // down
                        counter = 1;
                        while (r + counter < 8 && virtual_board[r + counter, c] == "")
                        {
                            counter++;
                        }
                        if (r + counter < 8)
                        {
                            entry = virtual_board[r + counter, c];
                            if (entry.Substring(0, 1) != Turn
                                && (entry.Substring(6) == "R"
                                || entry.Substring(6) == "Q"))
                            {
                                result = true;
                            }
                        }
                        // up
                        counter = 1;
                        while (r - counter >= 0 && virtual_board[r - counter, c] == "")
                        {
                            counter++;
                        }
                        if (r - counter >= 0)
                        {
                            entry = virtual_board[r - counter, c];
                            if (entry.Substring(0, 1) != Turn
                                && (entry.Substring(6) == "R"
                                || entry.Substring(6) == "Q"))
                            {
                                result = true;
                            }
                        }
                        // right
                        counter = 1;
                        while (c + counter < 8 && virtual_board[r, c + counter] == "")
                        {
                            counter++;
                        }
                        if (c + counter < 8)
                        {
                            entry = virtual_board[r, c + counter];
                            if (entry.Substring(0, 1) != Turn
                                && (entry.Substring(6) == "R"
                                || entry.Substring(6) == "Q"))
                            {
                                result = true;
                            }
                        }
                        // left
                        counter = 1;
                        while (c - counter >= 0 && virtual_board[r, c - counter] == "")
                        {
                            counter++;
                        }
                        if (c - counter >= 0)
                        {
                            entry = virtual_board[r, c - counter];
                            if (entry.Substring(0, 1) != Turn
                                && (entry.Substring(6) == "R"
                                || entry.Substring(6) == "Q"))
                            {
                                result = true;
                            }
                        }
                        #endregion

                        #region Find B or Q

                        for (int a = -1; a <= 1; a += 2)
                        {
                            for (int b = -1; b <= 1; b += 2)
                            {
                                counter = 1;

                                while (r + a * counter < 8
                                    && c + b * counter < 8
                                    && r + a * counter >= 0
                                    && c + b * counter >= 0
                                    && virtual_board[r + a * counter, c + b * counter] == "")
                                {
                                    counter++;
                                }
                                if (r + a * counter < 8
                                    && c + b * counter < 8
                                    && r + a * counter >= 0
                                    && c + b * counter >= 0)
                                {
                                    entry = virtual_board[r + a * counter, c + b * counter];
                                    if (entry.Substring(0, 1) != Turn
                                        && (entry.Substring(6) == "B"
                                        || entry.Substring(6) == "Q"))
                                    {
                                        result = true;
                                    }
                                }
                            }
                        }

                        //**

                        #endregion

                        #region Find N
                        counter = -1;
                        counter2 = 2;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        counter = 1;
                        counter2 = 2;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        counter = 2;
                        counter2 = 1;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        counter = 2;
                        counter2 = -1;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        counter = -1;
                        counter2 = -2;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        counter = 1;
                        counter2 = -2;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        counter = -2;
                        counter2 = 1;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        counter = -2;
                        counter2 = -1;

                        if ((0 <= r + counter && r + counter < 8)
                            && (0 <= c + counter2 && c + counter2 < 8)
                            && (virtual_board[r + counter, c + counter2].Length > 0
                            && virtual_board[r + counter, c + counter2].Substring(0, 1) != Turn
                            && virtual_board[r + counter, c + counter2].Substring(6, 1) == "N"))
                        {
                            result = true;
                        }

                        #endregion

                        #region Find P

                        if (Turn == "W")
                        {
                            // check above
                            if (r - 1 >= 0 && c - 1 >= 0
                                && (virtual_board[r - 1, c - 1].Length > 0)
                                && virtual_board[r - 1, c - 1].Substring(0, 1) != Turn
                                && (virtual_board[r - 1, c - 1].Substring(6) == "P"))
                            {
                                result = true;
                            }

                            else if (r - 1 >= 0 && c + 1 < 8
                                && (virtual_board[r - 1, c + 1].Length > 0)
                                && virtual_board[r - 1, c + 1].Substring(0, 1) != Turn
                                && (virtual_board[r - 1, c + 1].Substring(6) == "P"))
                            {
                                result = true;
                            }
                        }
                        else if (Turn == "B")
                        {
                            // check above
                            if (r + 1 < 8 && c - 1 >= 0
                                && (virtual_board[r + 1, c - 1].Length > 0)
                                && virtual_board[r + 1, c - 1].Substring(0, 1) != Turn
                                && (virtual_board[r + 1, c - 1].Substring(6) == "P"))
                            {
                                result = true;
                            }

                            else if (r + 1 < 8 && c + 1 < 8
                                && (virtual_board[r + 1, c + 1].Length > 0)
                                && virtual_board[r + 1, c + 1].Substring(0, 1) != Turn
                                && (virtual_board[r + 1, c + 1].Substring(6) == "P"))
                            {
                                result = true;
                            }
                        }

                        #endregion

                        #region Find K

                        for (int horizontal = -1; horizontal <= 1; horizontal++)
                        {
                            for (int vertical = -1; vertical <= 1; vertical++)
                            {
                                if ((horizontal != 0 || vertical != 0)
                                    && (0 <= r + vertical && r + vertical < 8)
                                    && (0 <= c + horizontal && c + horizontal < 8)
                                    && (virtual_board[r + vertical, c + horizontal].Length > 0)
                                    && (virtual_board[r + vertical, c + horizontal].Substring(0, 1) != Turn)
                                    && (virtual_board[r + vertical, c + horizontal].Substring(6) == "K"))
                                {
                                    result = true;
                                }
                            }
                        }

                        #endregion
                    }
                }
            }

            return result;
        }

        public void UndoMove(int old_x, int old_y, int new_x, int new_y, 
            string taken_piece, bool promotion, bool en_passant,
            bool left_white_castle_moved_1, bool right_white_castle_moved_1,
            bool left_black_castle_moved_1, bool right_black_castle_moved_1)
        {
            // undo move
            int r1 = old_y;
            int c1 = old_x;
            int r2 = new_y;
            int c2 = new_x;

            // virtual move
            Position[r1, c1] = Position[r2, c2];

            // taken piece
            Position[r2, c2] = taken_piece;

            if (taken_piece != "")
            {
                piece_count++;
            }

            // en passant
            if (en_passant)
            {
                piece_count++;
                fast_pawn_column = c2;
                if (Turn == "W")
                {
                    // 3 row
                    Position[4, c2] = "White P";
                }
                else
                {
                    // 4 row
                    Position[3, c2] = "Black P";
                }
            }
            else
            {
                fast_pawn_column = -1;
            }

            // castle
            if (Position[r1, c1] == "White K")
            {
                if (c2 - c1 == 2)
                {
                    // castle kingside
                    Position[7, 7] = Position[7, 5];
                    Position[7, 5] = "";
                }
                else if (c2 - c1 == -2)
                {
                    // castle queenside
                    Position[7, 0] = Position[7, 3];
                    Position[7, 3] = "";
                }
            }
            else if (Position[r1, c1] == "Black K")
            {
                if (c2 - c1 == 2)
                {
                    // castle kingside
                    Position[0, 7] = Position[0, 5];
                    Position[0, 5] = "";
                }
                else if (c2 - c1 == -2)
                {
                    // castle queenside
                    Position[0, 0] = Position[0, 3];
                    Position[0, 3] = "";
                }
            }

            // promotion
            if (promotion)
            {
                if (Turn == "W")
                {
                    Position[r1, c1] = "Black P";
                }
                else
                {
                    Position[r1, c1] = "White P";
                }
            }

            // other var
            left_white_castle_moved = left_white_castle_moved_1;
            right_white_castle_moved = right_white_castle_moved_1;
            left_black_castle_moved = left_black_castle_moved_1;
            right_black_castle_moved = right_black_castle_moved_1;

            ChangeTurn();

            UpdatePossibleMoves();
        }



        public ChessMove String_To_Move(string move)
        {
            ChessMove result;

            result.old_x = Convert.ToInt32(move.Substring(3, 1));
            result.old_y = Convert.ToInt32(move.Substring(1, 1));

            result.new_x = Convert.ToInt32(move.Substring(12, 1));
            result.new_y = Convert.ToInt32(move.Substring(10, 1));

            result.taken_piece = Position[result.new_y, result.new_x];

            result.moved_piece = Position[result.old_y, result.old_x].Substring(6, 1);

            result.move_notation = "";

            result.en_passant = result.old_x != result.new_x
                && result.moved_piece == "P"
                && result.taken_piece == "";

            if (result.moved_piece == "P" && result.taken_piece != "")
            {
                result.move_notation = Num_To_Char(result.old_x);
            }
            else if (result.moved_piece != "P")
            {
                result.move_notation = result.moved_piece;
            }

            if (result.taken_piece != "")
            {
                result.move_notation += "x";
            }

            result.move_notation += result.move_notation = Num_To_Char(result.new_x)
                    + (8 - result.new_y).ToString();

            result.promotion = move.Length > 14;

            result.left_white_castle_moved = left_white_castle_moved;
            result.right_white_castle_moved = right_white_castle_moved;
            result.left_black_castle_moved = left_black_castle_moved;
            result.right_black_castle_moved = right_black_castle_moved;

            return result;
        }

        string Num_To_Char(int num)
        {
            if (num == 0)
            {
                return "a";
            }
            else if (num == 1)
            {
                return "b";
            }
            else if (num == 2)
            {
                return "c";
            }
            else if (num == 3)
            {
                return "d";
            }
            else if (num == 4)
            {
                return "e";
            }
            else if (num == 5)
            {
                return "f";
            }
            else if (num == 6)
            {
                return "g";
            }
            else
            {
                return "h";
            }
        }

        public void UpdatePossibleMoves()
        {
            string entry;
            string piece;
            int counter = 0;
            int move_count = 0;
            string[] temp_possible_moves = new string[200];

            // Reset possible_moves
            for (int i = 0; i < 200; i++)
            {
                temp_possible_moves[i] = "";
            }

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    entry = Position[r, c];

                    if (entry.Length > 0 && entry.Substring(0, 1) == Turn)
                    {
                        piece = entry.Substring(6);

                        #region King
                        if (piece == "K")
                        {
                            for (int horizontal = -1; horizontal <= 1; horizontal++)
                            {
                                for (int vertical = -1; vertical <= 1; vertical++)
                                {
                                    if ((horizontal != 0 || vertical != 0)
                                        && (0 <= r + vertical && r + vertical < 8)
                                        && (0 <= c + horizontal && c + horizontal < 8)
                                        && (Position[r + vertical, c + horizontal] == ""
                                        || Position[r + vertical, c + horizontal].Substring(0, 1) != Turn))
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + vertical).ToString() + " "
                                            + (c + horizontal).ToString() + ")";
                                        move_count++;
                                    }
                                }
                            }

                            // castle
                            if (Turn == "W")
                            {
                                if (!left_white_castle_moved
                                    && Position[7, 1] == ""
                                    && Position[7, 2] == ""
                                    && Position[7, 3] == ""
                                    && CheckVirtualCheck(null) == false
                                    && CheckVirtualCheck("(7 4) -> (7 3)") == false)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + r.ToString() + " "
                                            + (c - 2).ToString() + ")";
                                    move_count++;
                                }
                                if (!right_white_castle_moved
                                    && Position[7, 5] == ""
                                    && Position[7, 6] == ""
                                    && CheckVirtualCheck(null) == false
                                    && CheckVirtualCheck("(7 4) -> (7 5)") == false)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + r.ToString() + " "
                                            + (c + 2).ToString() + ")";
                                    move_count++;
                                }
                            }
                            else
                            {
                                if (!left_black_castle_moved
                                    && Position[0, 1] == ""
                                    && Position[0, 2] == ""
                                    && Position[0, 3] == ""
                                    && CheckVirtualCheck(null) == false
                                    && CheckVirtualCheck("(0 4) -> (0 3)") == false)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + r.ToString() + " "
                                            + (c - 2).ToString() + ")";
                                    move_count++;
                                }
                                if (!right_black_castle_moved
                                    && Position[0, 5] == ""
                                    && Position[0, 6] == ""
                                    && CheckVirtualCheck(null) == false
                                    && CheckVirtualCheck("(0 4) -> (0 5)") == false)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + r.ToString() + " "
                                            + (c + 2).ToString() + ")";
                                    move_count++;
                                }
                            }
                        }
                        #endregion

                        #region Queen
                        else if (piece == "Q")
                        {
                            for (int horizontal = -1; horizontal <= 1; horizontal++)
                            {
                                for (int vertical = -1; vertical <= 1; vertical++)
                                {
                                    counter = 1;

                                    while ((horizontal != 0 || vertical != 0)
                                        && (0 <= r + vertical * counter
                                            && r + vertical * counter < 8)
                                            && (0 <= c + horizontal * counter
                                            && c + horizontal * counter < 8)
                                        && Position[r + vertical * counter, c + horizontal * counter] == "")
                                    {
                                        if ((Position[r + vertical * counter, c + horizontal * counter] == ""
                                            || Position[r + vertical * counter, c + horizontal * counter]
                                            .Substring(0, 1) != Turn))
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                                + c.ToString() + ") -> (" + (r + vertical * counter).ToString() + " "
                                                + (c + horizontal * counter).ToString() + ")";
                                            move_count++;
                                        }

                                        counter++;
                                    }

                                    if ((horizontal != 0 || vertical != 0)
                                        && (0 <= r + vertical * counter
                                            && r + vertical * counter < 8)
                                            && (0 <= c + horizontal * counter
                                            && c + horizontal * counter < 8)
                                            && (Position[r + vertical * counter, c + horizontal * counter] != ""
                                            && Position[r + vertical * counter, c + horizontal * counter]
                                            .Substring(0, 1) != Turn))
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + vertical * counter).ToString() + " "
                                            + (c + horizontal * counter).ToString() + ")";
                                        move_count++;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Rook
                        else if (piece == "R")
                        {
                            for (int horizontal = -1; horizontal <= 1; horizontal++)
                            {
                                for (int vertical = -1; vertical <= 1; vertical++)
                                {
                                    counter = 1;

                                    while ((horizontal == 0 || vertical == 0)
                                        && (0 <= r + vertical * counter
                                            && r + vertical * counter < 8)
                                            && (0 <= c + horizontal * counter
                                            && c + horizontal * counter < 8)
                                        && Position[r + vertical * counter, c + horizontal * counter] == "")
                                    {
                                        if ((Position[r + vertical * counter, c + horizontal * counter] == ""
                                            || Position[r + vertical * counter, c + horizontal * counter]
                                            .Substring(0, 1) != Turn))
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                                + c.ToString() + ") -> (" + (r + vertical * counter).ToString() + " "
                                                + (c + horizontal * counter).ToString() + ")";
                                            move_count++;
                                        }

                                        counter++;
                                    }

                                    if ((horizontal == 0 || vertical == 0)
                                        && (0 <= r + vertical * counter
                                            && r + vertical * counter < 8)
                                            && (0 <= c + horizontal * counter
                                            && c + horizontal * counter < 8)
                                            && (Position[r + vertical * counter, c + horizontal * counter] != ""
                                            && Position[r + vertical * counter, c + horizontal * counter]
                                            .Substring(0, 1) != Turn))
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + vertical * counter).ToString() + " "
                                            + (c + horizontal * counter).ToString() + ")";
                                        move_count++;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Bishop
                        else if (piece == "B")
                        {
                            for (int horizontal = -1; horizontal <= 1; horizontal++)
                            {
                                for (int vertical = -1; vertical <= 1; vertical++)
                                {
                                    counter = 1;

                                    while ((horizontal != 0 && vertical != 0)
                                        && (0 <= r + vertical * counter
                                            && r + vertical * counter < 8)
                                            && (0 <= c + horizontal * counter
                                            && c + horizontal * counter < 8)
                                        && Position[r + vertical * counter, c + horizontal * counter] == "")
                                    {
                                        if ((Position[r + vertical * counter, c + horizontal * counter] == ""
                                            || Position[r + vertical * counter, c + horizontal * counter]
                                            .Substring(0, 1) != Turn))
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                                + c.ToString() + ") -> (" + (r + vertical * counter).ToString() + " "
                                                + (c + horizontal * counter).ToString() + ")";
                                            move_count++;
                                        }

                                        counter++;
                                    }

                                    if ((horizontal != 0 && vertical != 0)
                                        && (0 <= r + vertical * counter
                                            && r + vertical * counter < 8)
                                            && (0 <= c + horizontal * counter
                                            && c + horizontal * counter < 8)
                                            && (Position[r + vertical * counter, c + horizontal * counter] != ""
                                            && Position[r + vertical * counter, c + horizontal * counter]
                                            .Substring(0, 1) != Turn))
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + vertical * counter).ToString() + " "
                                            + (c + horizontal * counter).ToString() + ")";
                                        move_count++;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Knight
                        else if (piece == "N")
                        {
                            // Part 1
                            for (int horizontal = -1; horizontal <= 1; horizontal += 2)
                            {
                                for (int vertical = -2; vertical <= 2; vertical += 4)
                                {
                                    if ((0 <= r + vertical && r + vertical < 8)
                                        && (0 <= c + horizontal && c + horizontal < 8)
                                        && (Position[r + vertical, c + horizontal] == ""
                                        || Position[r + vertical, c + horizontal].Substring(0, 1) != Turn))
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + vertical).ToString() + " "
                                            + (c + horizontal).ToString() + ")";
                                        move_count++;
                                    }
                                }
                            }
                            // Part 2
                            for (int horizontal = -2; horizontal <= 2; horizontal += 4)
                            {
                                for (int vertical = -1; vertical <= 1; vertical += 2)
                                {
                                    if ((0 <= r + vertical && r + vertical < 8)
                                        && (0 <= c + horizontal && c + horizontal < 8)
                                        && (Position[r + vertical, c + horizontal] == ""
                                        || Position[r + vertical, c + horizontal].Substring(0, 1) != Turn))
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + vertical).ToString() + " "
                                            + (c + horizontal).ToString() + ")";
                                        move_count++;
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Pawn
                        else if (piece == "P")
                        {
                            if (Turn == "W")
                            {
                                // forward 2
                                if (r == 6
                                    && Position[r - 1, c] == ""
                                    && Position[r - 2, c] == "")
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 2).ToString() + " "
                                            + (c).ToString() + ")";
                                    move_count++;
                                }

                                // forward 1
                                if (r - 1 >= 0
                                    && Position[r - 1, c] == "")
                                {
                                    if (r > 1)
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c).ToString() + ")";
                                        move_count++;
                                    }
                                    else
                                    {
                                        // promotion options
                                        for (int i = 0; i < promotion_options.Length; i++)
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c).ToString() + ") -> " + promotion_options[i];
                                            move_count++;
                                        }
                                    }
                                }

                                // take left
                                if (r - 1 >= 0
                                    && c - 1 >= 0
                                    && Position[r - 1, c - 1] != ""
                                    && Position[r - 1, c - 1].Substring(0, 1) != Turn)
                                {
                                    if (r > 1)
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c - 1).ToString() + ")";
                                        move_count++;
                                    }
                                    else
                                    {
                                        // promotion options
                                        for (int i = 0; i < promotion_options.Length; i++)
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c - 1).ToString() + ") -> " + promotion_options[i];
                                            move_count++;
                                        }
                                    }
                                }

                                // take right
                                if (r - 1 >= 0
                                    && c + 1 < 8
                                    && Position[r - 1, c + 1] != ""
                                    && Position[r - 1, c + 1].Substring(0, 1) != Turn)
                                {
                                    if (r > 1)
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c + 1).ToString() + ")";
                                        move_count++;
                                    }
                                    else
                                    {
                                        // promotion options
                                        for (int i = 0; i < promotion_options.Length; i++)
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c + 1).ToString() + ") -> " + promotion_options[i];
                                            move_count++;
                                        }
                                    }
                                }

                                // en passant left
                                if (r == 3
                                    && fast_pawn_column != -1
                                    && c - fast_pawn_column == 1)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c - 1).ToString() + ")";
                                    move_count++;
                                }

                                // en passant right
                                if (r == 3
                                    && fast_pawn_column != -1
                                    && c - fast_pawn_column == -1)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r - 1).ToString() + " "
                                            + (c + 1).ToString() + ")";
                                    move_count++;
                                }
                            }
                            else
                            {
                                // forward 2
                                if (r == 1
                                    && Position[r + 1, c] == ""
                                    && Position[r + 2, c] == "")
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 2).ToString() + " "
                                            + (c).ToString() + ")";
                                    move_count++;
                                }

                                // forward 1
                                if (r - 1 >= 0
                                    && Position[r + 1, c] == "")
                                {
                                    if (r < 6)
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c).ToString() + ")";
                                        move_count++;
                                    }
                                    else
                                    {
                                        // promotion options
                                        for (int i = 0; i < promotion_options.Length; i++)
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c).ToString() + ") -> " + promotion_options[i];
                                            move_count++;
                                        }
                                    }
                                }

                                // take left
                                if (r + 1 < 8
                                    && c - 1 >= 0
                                    && Position[r + 1, c - 1] != ""
                                    && Position[r + 1, c - 1].Substring(0, 1) == "W")
                                {
                                    if (r < 6)
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c - 1).ToString() + ")";
                                        move_count++;
                                    }
                                    else
                                    {
                                        // promotion options
                                        for (int i = 0; i < promotion_options.Length; i++)
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c - 1).ToString() + ") -> " + promotion_options[i];
                                            move_count++;
                                        }
                                    }
                                }

                                // take right
                                if (r + 1 < 8
                                    && c + 1 < 8
                                    && Position[r + 1, c + 1] != ""
                                    && Position[r + 1, c + 1].Substring(0, 1) == "W")
                                {
                                    if (r < 6)
                                    {
                                        temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c + 1).ToString() + ")";
                                        move_count++;
                                    }
                                    else
                                    {
                                        // promotion options
                                        for (int i = 0; i < promotion_options.Length; i++)
                                        {
                                            temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c + 1).ToString() + ") -> " + promotion_options[i];
                                            move_count++;
                                        }
                                    }
                                }

                                // en passant left
                                if (r == 4
                                    && fast_pawn_column != -1
                                    && c - fast_pawn_column == 1)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c - 1).ToString() + ")";
                                    move_count++;
                                }

                                // en passant right
                                if (r == 4
                                    && fast_pawn_column != -1
                                    && c - fast_pawn_column == -1)
                                {
                                    temp_possible_moves[move_count] = "(" + r.ToString() + " "
                                            + c.ToString() + ") -> (" + (r + 1).ToString() + " "
                                            + (c + 1).ToString() + ")";
                                    move_count++;
                                }
                            }
                        }
                        #endregion
                    }
                }
            }

            // eliminate in-check moves
            for (int i = 0; i < move_count; i++)
            {
                if (CheckVirtualCheck(temp_possible_moves[i]) == true)
                {
                    temp_possible_moves[i] = "";
                }
            }

            // eliminate spaces between moves
            counter = 0;
            for (int i = 0; i < move_count; i++)
            {
                if (temp_possible_moves[i] != "")
                {
                    temp_possible_moves[counter] = temp_possible_moves[i];

                    if (i > counter)
                        temp_possible_moves[i] = "";

                    counter++;
                }
            }
            move_count = counter;

            Possible_Moves = new string[move_count];

            for (int i = 0; i < move_count; i++)
            {
                Possible_Moves[i] = temp_possible_moves[i];
            }
        }

        public bool Checkmate()
        {
            if (Possible_Moves.Length == 0
                && CheckVirtualCheck(null))
            {
                return true;
            }
            else { return false; }
        }

        public bool Stalemate()
        {
            if (Possible_Moves.Length == 0
                && !CheckVirtualCheck(null))
            {
                return true;
            }
            else { return false; }

        }

        public Point[] Attacked_Squares(int x, int y)
        {
            if (Position[y, x].Length < 7)
            {
                return null;
            }

            string piece = Position[y, x].Substring(6, 1);
            string colour = Position[y, x].Substring(0, 1); // colour moved here

            Point[] result = null;
            Point[] temp = new Point[100];
            int counter = 0;

            #region Pawn

            if (piece == "P")
            {
                if (colour == "W")
                {
                    if (x != 0)
                    {
                        temp[counter] = new Point(x - 1, y - 1);
                        counter++;
                    }
                    if (x != 7)
                    {
                        temp[counter] = new Point(x + 1, y - 1);
                        counter++;
                    }
                }
                else
                {
                    if (x != 0)
                    {
                        temp[counter] = new Point(x - 1, y + 1);
                        counter++;
                    }
                    if (x != 7)
                    {
                        temp[counter] = new Point(x + 1, y + 1);
                        counter++;
                    }
                }
            }

            #endregion

            #region Knight

            else if (piece == "N")
            {
                if (y > 0)
                {
                    if (x < 6)
                    {
                        temp[counter] = new Point(x + 2, y - 1);
                        counter++;
                    }
                    if (x > 1)
                    {
                        temp[counter] = new Point(x - 2, y - 1);
                        counter++;
                    }

                    if (y > 1)
                    {
                        if (x < 7)
                        {
                            temp[counter] = new Point(x + 1, y - 2);
                            counter++;
                        }
                        if (x > 0)
                        {
                            temp[counter] = new Point(x - 1, y - 2);
                            counter++;
                        }
                    }
                }

                if (y < 7)
                {
                    if (x < 6)
                    {
                        temp[counter] = new Point(x + 2, y + 1);
                        counter++;
                    }
                    if (x > 1)
                    {
                        temp[counter] = new Point(x - 2, y + 1);
                        counter++;
                    }

                    if (y < 6)
                    {
                        if (x < 7)
                        {
                            temp[counter] = new Point(x + 1, y + 2);
                            counter++;
                        }
                        if (x > 0)
                        {
                            temp[counter] = new Point(x - 1, y + 2);
                            counter++;
                        }
                    }
                }
            }

            #endregion

            #region Bishop or Queen

            else if (piece == "B" || piece == "Q")
            {
                for (int r = -1; r <= 1; r += 2)
                {
                    for (int c = -1; c <= 1; c += 2)
                    {
                        int i = 1;

                        while (0 <= x + i * c && x + i * c <= 7
                            && 0 <= y + i * r && y + i * r <= 7)
                        {
                            temp[counter] = new Point(x + i * c, y + i * r);
                            counter++;

                            if (Position[y + i * r, x + i * c] != "")
                            {
                                break;
                            }

                            i++;
                        }
                    }
                }
            }

            #endregion

            #region Rook or Queen

            if (piece == "R" || piece == "Q")
            {
                for (int r = -1; r <= 1; r += 2)
                {
                    int i = 1;

                    while (0 <= y + i * r && y + i * r <= 7)
                    {
                        temp[counter] = new Point(x, y + i * r);
                        counter++;

                        if (Position[y + i * r, x] != "")
                        {
                            break;
                        }

                        i++;
                    }
                }

                for (int c = -1; c <= 1; c += 2)
                {
                    int i = 1;

                    while (0 <= x + i * c && x + i * c <= 7)
                    {
                        temp[counter] = new Point(x + i * c, y);
                        counter++;

                        if (Position[y, x + i * c] != "")
                        {
                            break;
                        }

                        i++;
                    }
                }
            }

            #endregion

            #region King

            else if (piece == "K")
            {
                for (int r = -1; r <= 1; r++)
                {
                    for (int c = -1; c <= 1; c++)
                    {
                        if ((r != 0 || c != 0)
                            && 0 <= y + r && y + r < 8
                            && 0 <= x + c && x + c < 8)
                        {
                            temp[counter] = new Point(x + c, y + r);
                            counter++;
                        }
                    }
                }
            }

            #endregion

            result = new Point[counter];
            for (int i = 0; i < counter; i++)
            {
                result[i] = temp[i];
            }

            return result;
        }

        public int Pin_Move(ChessMove candidate, int field_point)
        {
            // -1 means not a pin move
            // 0 means a pin move that does not result in material gain
            // > 0 means a pin move that results in material gain

            int result = -1;
            // definition in code:
            // * pinned piece is knight, bishop, rook, queen
            // * pinned piece is worth more than piece behind it
            // * piece behind is worth more than piece used for pin
            // * pinned piece must not be the same as the piece used for the pin

            string first_piece = null;
            int first_piece_worth = -1;
            int counter = 1;
            int x = candidate.new_x;
            int y = candidate.new_y;

            if (candidate.moved_piece == "R" || candidate.moved_piece == "Q")
            {
                // look for knight, bishop, or queen
                first_piece_worth = -1;

                // rows
                for (int r = -1; r < 2; r += 2)
                {
                    counter = 1;
                    first_piece_worth = -1;
                    first_piece = null;

                    while (0 <= y + r * counter && y + r * counter < 8)
                    {
                        if (this.Position[y + r * counter, x] != "")
                        {
                            if (first_piece_worth == -1)
                            {
                                first_piece = this
                                    .Position[y + r * counter, x].Substring(6, 1);
                                first_piece_worth = PieceWorth(first_piece);

                                if (first_piece == candidate.moved_piece
                                    || this.Position[y + r * counter, x].Substring(0, 1) != Turn
                                    || first_piece_worth == 1)
                                {
                                    break;
                                }
                            }
                            else if (PieceWorth(this.Position[y + r * counter, x].Substring(6, 1)) >
                                    first_piece_worth)
                            {
                                if (this.Position[y + r * counter, x].Substring(0, 1) == Turn)
                                {
                                    break;
                                }

                                if (result == -1) result = 0;

                                if (PieceWorth(candidate.moved_piece) < first_piece_worth
                                    && (field_point == 0 || field_point > PieceNum(candidate.moved_piece))) // ***
                                {
                                    result += first_piece_worth - PieceWorth(candidate.moved_piece);
                                }

                                break;
                            }
                        }

                        counter++;
                    }
                }

                // columns
                for (int c = -1; c < 2; c += 2)
                {
                    counter = 1;
                    first_piece_worth = -1;
                    first_piece = null;

                    while (0 <= x + c * counter && x + c * counter < 8)
                    {
                        if (this.Position[y, x + c * counter] != "")
                        {
                            if (first_piece_worth == -1)
                            {
                                first_piece = this
                                    .Position[y, x + c * counter].Substring(6, 1);
                                first_piece_worth = PieceWorth(first_piece);

                                if (first_piece == candidate.moved_piece
                                    || this.Position[y, x + c * counter].Substring(0, 1) != Turn
                                    || first_piece_worth == 1)
                                {
                                    break;
                                }
                            }
                            else if (PieceWorth(this.Position[y, x + c * counter].Substring(6, 1)) >
                                    first_piece_worth)
                            {
                                if (this.Position[y, x + c * counter].Substring(0, 1) == Turn)
                                {
                                    break;
                                }

                                if (result == -1) result = 0;

                                if (PieceWorth(candidate.moved_piece) < first_piece_worth
                                    && (field_point == 0 || field_point > PieceNum(candidate.moved_piece))) // ***
                                {
                                    result += first_piece_worth - PieceWorth(candidate.moved_piece);
                                }

                                break;
                            }
                        }

                        counter++;
                    }
                }
            }
            if (candidate.moved_piece == "B" || candidate.moved_piece == "Q")
            {
                // look for knight, rook, or queen

                for (int r = -1; r < 2; r += 2)
                {
                    for (int c = -1; c < 2; c += 2)
                    {
                        counter = 1;
                        first_piece_worth = -1;
                        first_piece = null;

                        while (0 <= y + r * counter && y + r * counter < 8
                            && 0 <= x + c * counter && x + c * counter < 8)
                        {
                            if (this.Position[y + r * counter, x + c * counter] != "")
                            {
                                if (first_piece_worth == -1)
                                {
                                    first_piece = this
                                        .Position[y + r * counter, x + c * counter].Substring(6, 1);
                                    first_piece_worth = PieceWorth(first_piece);

                                    if (first_piece == candidate.moved_piece
                                        || first_piece_worth == 1)
                                    {
                                        break;
                                    }
                                }
                                else if (PieceWorth(this.Position[y + r * counter, x + c * counter].Substring(6, 1)) >
                                    first_piece_worth)
                                {
                                    if (result == -1) result = 0;
                                    
                                    if (PieceWorth(candidate.moved_piece) < first_piece_worth
                                        && (field_point == 0 || field_point > PieceNum(candidate.moved_piece))) // ***
                                    {
                                        result += first_piece_worth - PieceWorth(candidate.moved_piece);
                                    }

                                    break;
                                }
                            }

                            counter++;
                        }
                    }
                }
            }
            
            return result;
        }

        public int Skewer_Move(ChessMove candidate, int field_point)
        {
            int result = 0;
            
            // definition in code:
            // * pinned piece is knight, bishop, rook, queen
            // * pinned piece is worth less than or equal to than piece behind it
            // * piece attacked is worth more than to the piece used for pin
            // * pinned piece must not be the same as the piece used for the pin

            string first_piece = null;
            int first_piece_worth = -1;
            int counter = 1;
            int x = candidate.new_x;
            int y = candidate.new_y;

            if (candidate.moved_piece == "R" || candidate.moved_piece == "Q")
            {
                // look for knight, bishop, or queen
                first_piece_worth = -1;

                // rows
                for (int r = -1; r < 2; r += 2)
                {
                    counter = 1;
                    first_piece_worth = -1;
                    first_piece = null;

                    while (0 <= y + r * counter && y + r * counter < 8)
                    {
                        if (this.Position[y + r * counter, x] != "")
                        {
                            if (first_piece_worth == -1)
                            {
                                first_piece = this
                                    .Position[y + r * counter, x].Substring(6, 1);
                                first_piece_worth = PieceWorth(first_piece);

                                if (first_piece == candidate.moved_piece
                                    || this.Position[y + r * counter, x].Substring(0, 1) == Turn
                                    || first_piece_worth <= PieceWorth(candidate.moved_piece)) // **
                                {
                                    break;
                                }
                            }
                            else if (PieceWorth(this.Position[y + r * counter, x].Substring(6, 1)) <=
                                    first_piece_worth)
                            {
                                int piece_behind_worth = PieceWorth(this.Position[y + r * counter, x].Substring(6, 1));

                                if (this.Position[y + r * counter, x].Substring(0, 1) == Turn)
                                {
                                    break;
                                }

                                if (PieceWorth(candidate.moved_piece) < piece_behind_worth
                                    && (field_point == 0 || field_point > PieceNum(candidate.moved_piece))) // **
                                {
                                    result += piece_behind_worth - PieceWorth(candidate.moved_piece);
                                }

                                break;
                            }
                        }

                        counter++;
                    }
                }

                // columns
                for (int c = -1; c < 2; c += 2)
                {
                    counter = 1;
                    first_piece_worth = -1;
                    first_piece = null;

                    while (0 <= x + c * counter && x + c * counter < 8)
                    {
                        if (this.Position[y, x + c * counter] != "")
                        {
                            if (first_piece_worth == -1)
                            {
                                first_piece = this
                                    .Position[y, x + c * counter].Substring(6, 1);
                                first_piece_worth = PieceWorth(first_piece);

                                if (first_piece == candidate.moved_piece
                                    || this.Position[y, x + c * counter].Substring(0, 1) == Turn
                                    || first_piece_worth <= PieceWorth(candidate.moved_piece))
                                {
                                    break;
                                }
                            }
                            else if (PieceWorth(this.Position[y, x + c * counter].Substring(6, 1)) <=
                                    first_piece_worth)
                            {
                                int piece_behind_worth = PieceWorth(this.Position[y, x + c * counter].Substring(6, 1));

                                if (this.Position[y, x + c * counter].Substring(0, 1) == Turn)
                                {
                                    break;
                                }

                                if (PieceWorth(candidate.moved_piece) < piece_behind_worth
                                    && (field_point == 0 || field_point > PieceNum(candidate.moved_piece))) // ***
                                {
                                    result += piece_behind_worth - PieceWorth(candidate.moved_piece);
                                }

                                break;
                            }
                        }

                        counter++;
                    }
                }
            }
            if (candidate.moved_piece == "B" || candidate.moved_piece == "Q")
            {
                // look for knight, rook, or queen

                for (int r = -1; r < 2; r += 2)
                {
                    for (int c = -1; c < 2; c += 2)
                    {
                        counter = 1;
                        first_piece_worth = -1;
                        first_piece = null;

                        while (0 <= y + r * counter && y + r * counter < 8
                            && 0 <= x + c * counter && x + c * counter < 8)
                        {
                            if (this.Position[y + r * counter, x + c * counter] != "")
                            {
                                if (first_piece_worth == -1)
                                {
                                    first_piece = this
                                        .Position[y + r * counter, x + c * counter].Substring(6, 1);
                                    first_piece_worth = PieceWorth(first_piece);

                                    if (first_piece == candidate.moved_piece
                                    || first_piece_worth <= PieceWorth(candidate.moved_piece))
                                    {
                                        break;
                                    }
                                }
                                else if (PieceWorth(this.Position[y + r * counter, x + c * counter].Substring(6, 1)) <=
                                    first_piece_worth)
                                {
                                    int piece_behind_worth = PieceWorth(this.Position[y + r * counter, x + c * counter].Substring(6, 1));
                                                                       
                                    if (PieceWorth(candidate.moved_piece) < piece_behind_worth
                                        && (field_point == 0 || field_point > PieceNum(candidate.moved_piece))) // ***
                                    {
                                        result += piece_behind_worth - PieceWorth(candidate.moved_piece);
                                    }

                                    break;
                                }
                            }

                            counter++;
                        }
                    }
                }
            }

            return result;
        }

        public int Tempo_Fork(ChessMove candidate, Point[] control_squares, 
            int[,] white_material_field, int[,] black_material_field)
        {
            // ** incorporate trapping
            // move already made

            // -1 means not a tempo or fork move
            // 0 means a tempo move that does not result in material gain
            // > 0 means a tempo and fork move that results in material gain

            int result = -1;

            int attacked_piece_num = 0;
            int max_worth = 0;
            int second_max_worth = -1;
            int new_difference = -1;

            for (int j = 0; j < control_squares.Length; j++)
            {
                int x = control_squares[j].X;
                int y = control_squares[j].Y;

                if (this.Position[control_squares[j].Y, control_squares[j].X] != "")
                {
                    string piece = this.Position[control_squares[j].Y, control_squares[j].X];
                    string piece_colour = piece.Substring(0, 1);
                    piece = piece.Substring(6, 1);

                    if (piece_colour == this.Turn
                            && (this.Turn == "B" && white_material_field[y, x] == 0
                            || this.Turn == "W" && black_material_field[y, x] == 0))
                    {
                        result = 0;
                        new_difference = PieceWorth(piece);
                    }
                    else if (piece_colour == this.Turn
                        && PieceWorth(piece) > PieceWorth(candidate.moved_piece))
                    {
                        result = 0;

                        new_difference = PieceWorth(piece) - PieceWorth(candidate.moved_piece);
                    }

                    if (new_difference != -1)
                    {
                        attacked_piece_num++;

                        if (attacked_piece_num == 1)
                        {
                            max_worth = new_difference;
                        }
                        else if (new_difference >= max_worth)
                        {
                            second_max_worth = max_worth;
                            max_worth = new_difference;
                        }
                        else if (second_max_worth < new_difference
                            && new_difference < max_worth)
                        {
                            second_max_worth = new_difference;
                        }

                        new_difference = -1;
                    }
                }
            }

            // Fork
            if (attacked_piece_num >= 2)
            {
                result += second_max_worth;
            }

            return result;
        }


        public int PieceNum(string piece)
        {
            if (piece == "P")
            {
                return 1;
            }
            else if (piece == "N")
            {
                return 10;
            }
            else if (piece == "B")
            {
                return 10;
            }
            else if (piece == "R")
            {
                return 100;
            }
            else if (piece == "Q")
            {
                return 1000;
            }
            else if (piece == "K")
            {
                return 10000;
            }
            else
            {
                return 0;
            }
        }

        public int PieceWorth(string piece)
        {
            if (piece == "P")
            {
                return 1;
            }
            else if (piece == "N")
            {
                return 3;
            }
            else if (piece == "B")
            {
                return 3;
            }
            else if (piece == "R")
            {
                return 5;
            }
            else if (piece == "Q")
            {
                return 9;
            }
            else if (piece == "K")
            {
                return 500;
            }
            else
            {
                return 0;
            }
        }

        public int PieceNumToPieceWorth(int piece_num)
        {
            if (piece_num == 1)
            {
                return 1;
            }
            else if (piece_num == 10)
            {
                return 3;
            }
            else if (piece_num == 100)
            {
                return 5;
            }
            else if (piece_num == 1000)
            {
                return 9;
            }
            else if (piece_num == 10000)
            {
                return 500;
            }
            else
            {
                return 0;
            }
        }

        public int RightNonZero(int n)
        {
            if (n == 0) return 0;
            else if (n % 10 != 0)
            {
                return 1;
            }
            else
            {
                return 10 * RightNonZero(n / 10);
            }
        }

        public int DigitSum(int n)
        {
            int acc = n % 10;

            while (n != 0)
            {
                n /= 10;
                acc += n % 10;
            }

            return acc;
        }

        public Point[] GetPieceList(bool is_white)
        {
            string req_colour = is_white ? "W" : "B";
            int counter = 0;
            Point[] temp = new Point[16];

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (Position[r, c] != "" && Position[r, c].Substring(0, 1) == req_colour)
                    {
                        temp[counter] = new Point(c, r);
                        counter++;
                    }
                }
            }

            if (counter < 16)
            {
                return temp;
            }

            Point[] piece_list = new Point[counter];

            for (int i = 0; i < counter; i++)
            {
                piece_list[i] = temp[i];
            }

            return piece_list;
        }

        public int[,] GetMaterialField(bool is_white)
        {
            int[,] mat_field = new int[8, 8];

            // reset
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    mat_field[i, j] = 0;
                }
            }
                        
            Point[] piece = GetPieceList(is_white);

            int y = 0;
            int x = 0;
            Point[] attacked_squares = null;
            string str_piece = null;
            int points_num = 0;

            // White
            for (int k = 0; k < piece.Length; k++)
            {
                y = piece[k].Y;
                x = piece[k].X;

                if (Position[y, x] != ""
                    && (is_white && Position[y, x].Substring(0, 1) == "W"
                        || !is_white && Position[y, x].Substring(0, 1) == "B")) 
                    // <- ** shouldn't even be here
                {
                    attacked_squares = Attacked_Squares(x, y);

                    str_piece = Position[y, x].Substring(6, 1);
                    points_num = PieceNum(str_piece);

                    for (int j = 0; j < attacked_squares.Length; j++)
                    {
                        mat_field[attacked_squares[j].Y,
                            attacked_squares[j].X] += points_num;
                    }
                }
            }
            

            return mat_field;
        }

        // checkmate = possible_moves.Length == 0
        // && VirtualMove("(0 0) -> (0 0)")




        /*
            string firstText = "Hello";
            string secondText = "World";

            PointF firstLocation = new PointF(10f, 10f);
            PointF secondLocation = new PointF(10f, 50f);

            string imageFilePath = @"path\picture.bmp";
            Bitmap bitmap = (Bitmap)Image.FromFile(imageFilePath);//load the image file

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (Font arialFont = new Font("Arial", 10))
                {
                    graphics.DrawString(firstText, arialFont, Brushes.Blue, firstLocation);
                    graphics.DrawString(secondText, arialFont, Brushes.Red, secondLocation);
                }


            }
        */
    }
}
