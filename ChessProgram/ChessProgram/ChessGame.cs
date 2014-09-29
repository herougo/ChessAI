using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessProgram
{

    public struct ChessMove
    {
        public int old_x;
        public int old_y;
        public int new_x;
        public int new_y;

        public string moved_piece;

        public string taken_piece;
        public string move_notation;
        public bool promotion;
        public bool en_passant;

        public bool left_white_castle_moved;
        public bool right_white_castle_moved;
        public bool left_black_castle_moved;
        public bool right_black_castle_moved;
    }

    class ChessGame
    {
        public ChessBoard Board = null;
        public bool in_game = false;
        public bool player_turn = false;
        public string player_colour = null;
        public int move_count = 0;  // records # of moves have been played

        public bool ai_opponent = true;

        public AI Opponent = null;

        ChessMove[] move_list = new ChessMove[200];

        public string MoveListToNotation()
        {
            string result = "";

            if (move_count > 0)
            {
                result = move_list[0].move_notation;
            }
            for (int i = 1; i < move_count; i++)
            {
                result += "\n" + move_list[i].move_notation;
            }

            if (Board.Checkmate())
            {
                result += "#";
            }
            else if (Board.Stalemate())
            {
                // ? **********************
            }

            return result;
        }

        public void NewGame(string player_col, bool ai_opp)
        {
            string[,] board = new string[8, 8];

            #region Reset Board

            // black back pieces
            board[0, 0] = "Black R";
            board[0, 1] = "Black N";
            board[0, 2] = "Black B";
            board[0, 3] = "Black Q";
            board[0, 4] = "Black K";
            board[0, 5] = "Black B";
            board[0, 6] = "Black N";
            board[0, 7] = "Black R";

            // black pawns
            for (int c = 0; c < 8; c++)
            {
                board[1, c] = "Black P";
            }

            // blank space
            for (int r = 2; r <= 5; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    board[r, c] = "";
                }
            }

            // black pawns
            for (int c = 0; c < 8; c++)
            {
                board[6, c] = "White P";
            }

            // black back pieces
            board[7, 0] = "White R";
            board[7, 1] = "White N";
            board[7, 2] = "White B";
            board[7, 3] = "White Q";
            board[7, 4] = "White K";
            board[7, 5] = "White B";
            board[7, 6] = "White N";
            board[7, 7] = "White R";

            #endregion

            move_count = 0;

            Board = new ChessBoard(board, "W", -1, false, false, false, false, 32);

            if (ai_opp)
            {
                Opponent = new AI(null, Board.BoardCopy(), move_count);
            }
            ai_opponent = ai_opp;

            in_game = true;

            player_colour = player_col;
            player_turn = (player_col == "W");            
        }

        public void Move(string move)
        {
            move_list[move_count] = Board.String_To_Move(move);
            
            move_count++;

            Board.MakeMove(move);

            if (ai_opponent)
            {
                Opponent.Virtual_Board.MakeMove(move);
                Opponent.move_count++;
            }

            player_turn = !player_turn;
        }

        public void Undo_Move()
        {
            move_count--;

            player_turn = !player_turn;

            Board.UndoMove(move_list[move_count].old_x,
                move_list[move_count].old_y,
                move_list[move_count].new_x,
                move_list[move_count].new_y,
                move_list[move_count].taken_piece,
                move_list[move_count].promotion,
                move_list[move_count].en_passant,
                move_list[move_count].left_white_castle_moved,
                move_list[move_count].right_white_castle_moved,
                move_list[move_count].left_black_castle_moved,
                move_list[move_count].right_black_castle_moved);

            if (ai_opponent)
            {
                Opponent.Virtual_Board.UndoMove(move_list[move_count].old_x,
                    move_list[move_count].old_y,
                    move_list[move_count].new_x,
                    move_list[move_count].new_y,
                    move_list[move_count].taken_piece,
                    move_list[move_count].promotion,
                    move_list[move_count].en_passant,
                    move_list[move_count].left_white_castle_moved,
                    move_list[move_count].right_white_castle_moved,
                    move_list[move_count].left_black_castle_moved,
                    move_list[move_count].right_black_castle_moved);
                Opponent.move_count--;
            }

            // destroy move
            move_list[move_count].old_x = 0;
            move_list[move_count].old_y = 0;
            move_list[move_count].new_x = 0;
            move_list[move_count].new_y = 0;

            move_list[move_count].taken_piece = null;
            move_list[move_count].move_notation = null;
            move_list[move_count].promotion = false;

            move_list[move_count].en_passant = false;

            move_list[move_count].left_white_castle_moved = false;
            move_list[move_count].right_white_castle_moved = false;
            move_list[move_count].left_black_castle_moved = false;
            move_list[move_count].right_black_castle_moved = false;
        }

        public void AIMove()
        {
            int chosen_move = -1;

            chosen_move = Opponent.ChooseMove();

            Move(Board.Possible_Moves[chosen_move]);
        }
    }
}
