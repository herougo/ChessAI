using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessProgram
{
    class AI
    {
        public OpeningBook AI_Opening_Book = new OpeningBook(null);
        public ChessBoard Virtual_Board = null;
        public string[] Current_Possible_Moves = null;

        public int[] Possible_Moves_Points = null;
        public int[] Possible_Moves_Material = null;

        public Point[] White_Piece = null;
        public int White_Piece_Length = 0;
        public Point[] Black_Piece = null;
        public int Black_Piece_Length = 0;

        public int Material_Points = 0;

        public int[,] White_Material_Field = null;
        public int[,] Black_Material_Field = null;

        // ** taken out to visibly see moves
        public int max_material_score = 0;
        public int max_score = 0;
        public int max_occur = 1;

        public string[] move_stats = null; // ** to visibly show points breakdown (testing)

        // 1 digit: pawns
        // 2 digit: knight
        // 3 digit: bishop
        // 4 digit: rook
        // 5 digit: queen
        // 6 digit: king

        #region Point System

        public int develop_points = 5;
        public int castle_points = 3;
        public int knight_rim_points = -3;
        public int centre_control_points = 2;
        public int tempo_points = 2;
        public int move_middle_pawn_points = 8; // ** specific
        public int open_diagonal_points = 2;
        public int fianchetto_points = 2;
        public int rook_to_pawn_rank_points = 5;
        public int pin_points = 4;
        public int checkmate_setup_multiplier = 10;
        public int rook_to_semi_open_file_points = 3;
        public int rook_to_open_file_points = 5;

        #endregion

        public int move_count { get; set; }



        public AI(string[] opening_lines, ChessBoard Start_Position, int start_move_count)
        {
            ChangeOpeningBook(opening_lines);
            move_count = start_move_count;
            Virtual_Board = Start_Position;
        }

        public void ChangeOpeningBook(string[] lines)
        {
            AI_Opening_Book = new OpeningBook(lines);
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

        public int InArray(int n, int[] arr)
        {
            if (arr.Contains(n)) return 1;
            else { return 0; }
        }

        public void SetPiece()
        {
            // ** make more efficient
            #region Set Piece Co-ordinates

            // if (White_Material_Field == null)
            {
                int counter = 0;
                int counter2 = 0;

                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        if (Virtual_Board.Position[r, c] != "")
                        {
                            if (Virtual_Board.Position[r, c].Substring(0, 1) == "W")
                            {
                                counter++;
                            }
                            else
                            {
                                counter2++;
                            }
                        }
                    }
                }

                White_Piece = new Point[counter];
                Black_Piece = new Point[counter2];

                counter = 0;
                counter2 = 0;

                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        if (Virtual_Board.Position[r, c] != "")
                        {
                            if (Virtual_Board.Position[r, c].Substring(0, 1) == "W")
                            {
                                White_Piece[counter] = new Point(c, r);
                                counter++;
                            }
                            else
                            {
                                Black_Piece[counter2] = new Point(c, r);
                                counter2++;
                            }
                        }
                    }
                }

                White_Piece_Length = White_Piece.Length;
                Black_Piece_Length = Black_Piece.Length;
            }

            #endregion
        }

        public void SetMatField()
        {
            if (White_Material_Field == null)
            {
                White_Material_Field = new int[8, 8];
                Black_Material_Field = new int[8, 8];
            }

            // Reset
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    White_Material_Field[r, c] = 0;
                    Black_Material_Field[r, c] = 0;
                }
            }

        }

        #region AI Moves

        public int OpeningMove()
        {
            string current_board = Virtual_Board.BoardToString();

            // search opening book
            if (Virtual_Board.Turn == "W" && (move_count / 2)
                < AI_Opening_Book.white_moves_depth)
            {
                for (int i = 0; i < AI_Opening_Book.white_moves_width; i++)
                {
                    if (Virtual_Board.piece_count == AI_Opening_Book.White_Piece_Count[move_count / 2, i]
                        && AI_Opening_Book.White_Board[move_count / 2, i] == current_board)
                    {
                        return Virtual_Board.MoveToInt(AI_Opening_Book.White_Move[move_count / 2, i]);
                    }
                }
            }
            else if (Virtual_Board.Turn == "B" && (move_count / 2)
                        < AI_Opening_Book.black_moves_depth)
            {
                for (int i = 0; i < AI_Opening_Book.black_moves_width; i++)
                {
                    if (Virtual_Board.piece_count == AI_Opening_Book.Black_Piece_Count[move_count / 2, i]
                        && AI_Opening_Book.Black_Board[move_count / 2, i] == current_board)
                    {
                        return Virtual_Board.MoveToInt(AI_Opening_Book.Black_Move[move_count / 2, i]);
                    }
                }
            }

            return -1;
        }

        public int ChooseMove()
        {
            int chosen_move = -1;

            ChessMove candidate;
            ChessMove move_2;
            ChessMove move_3;

            string ai_colour = Virtual_Board.Turn;

            int ai_pawn_row = 1;
            int ai_back_row = 0;
            int ai_pawn_direction = 1;
            if (ai_colour == "W")
            {
                ai_pawn_row = 6;
                ai_back_row = 7;
                ai_pawn_direction = -1;
            }

            move_stats = null;

            // opening
            chosen_move = OpeningMove();

            if (chosen_move != -1) return chosen_move;

            SetPiece();

            // ** can combine Checkmate with rest to improve efficiency

            #region Checkmate

            for (int i = 0; i < Virtual_Board.Possible_Moves.Length; i++)
            {
                candidate = Virtual_Board.String_To_Move(Virtual_Board.Possible_Moves[i]);

                Virtual_Board.MakeMove(Virtual_Board.Possible_Moves[i]);

                if (Virtual_Board.Checkmate())
                {
                    Virtual_Board.UndoMove(candidate.old_x,
                        candidate.old_y,
                        candidate.new_x,
                        candidate.new_y,
                        candidate.taken_piece,
                        candidate.promotion,
                        candidate.en_passant,
                        candidate.left_white_castle_moved,
                        candidate.right_white_castle_moved,
                        candidate.left_black_castle_moved,
                        candidate.right_black_castle_moved);

                    return i;
                }

                Virtual_Board.UndoMove(candidate.old_x,
                    candidate.old_y,
                    candidate.new_x,
                    candidate.new_y,
                    candidate.taken_piece,
                    candidate.promotion,
                    candidate.en_passant,
                    candidate.left_white_castle_moved,
                    candidate.right_white_castle_moved,
                    candidate.left_black_castle_moved,
                    candidate.right_black_castle_moved);
            }

            #endregion

            // copy to Current_Possible_Moves
            Current_Possible_Moves = new string[Virtual_Board.Possible_Moves.Length];
            Possible_Moves_Points = new int[Current_Possible_Moves.Length];
            Possible_Moves_Material = new int[Current_Possible_Moves.Length];

            for (int i = 0; i < Virtual_Board.Possible_Moves.Length; i++)
            {
                Current_Possible_Moves[i] = Virtual_Board.Possible_Moves[i];
                Possible_Moves_Points[i] = 0;
                Possible_Moves_Material[i] = 0;
            }

            #region Checkmate in 2

            for (int i = 0; i < Current_Possible_Moves.Length; i++)
            {
                Virtual_Board.ChangeTurn();

                #region Backup
                string[,] backup1 = new string[8, 8];

                // copy board
                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        backup1[r, c] = Virtual_Board.Position[r, c];
                    }
                }
                #endregion

                // if this puts the opponent in check
                if (Virtual_Board.CheckVirtualCheck(Current_Possible_Moves[i]))
                {
                    Virtual_Board.ChangeTurn();

                    candidate = Virtual_Board.String_To_Move(Current_Possible_Moves[i]);

                    Virtual_Board.MakeMove(Current_Possible_Moves[i]);

                    // check if all move 2's lead to checkmate
                    int counter = 0;
                    bool all_checkmate = true;

                    while (all_checkmate && counter < Virtual_Board.Possible_Moves.Length)
                    {
                        all_checkmate = false;

                        move_2 = Virtual_Board.String_To_Move(Virtual_Board.Possible_Moves[counter]);

                        #region Backup
                        string[,] backup2 = new string[8, 8];

                        // copy board
                        for (int r = 0; r < 8; r++)
                        {
                            for (int c = 0; c < 8; c++)
                            {
                                backup2[r, c] = Virtual_Board.Position[r, c];
                            }
                        }
                        #endregion

                        Virtual_Board.MakeMove(Virtual_Board.Possible_Moves[counter]);

                        #region Find Final Checkmate Move

                        for (int k = 0; k < Virtual_Board.Possible_Moves.Length; k++)
                        {
                            move_3 = Virtual_Board.String_To_Move(Virtual_Board.Possible_Moves[k]);

                            #region Backup
                            string[,] backup3 = new string[8, 8];

                            // copy board
                            for (int r = 0; r < 8; r++)
                            {
                                for (int c = 0; c < 8; c++)
                                {
                                    backup3[r, c] = Virtual_Board.Position[r, c];
                                }
                            }
                            #endregion

                            Virtual_Board.MakeMove(Virtual_Board.Possible_Moves[k]);

                            if (Virtual_Board.Checkmate())
                            {
                                all_checkmate = true;
                            }

                            Virtual_Board.UndoMove(move_3.old_x,
                                move_3.old_y,
                                move_3.new_x,
                                move_3.new_y,
                                move_3.taken_piece,
                                move_3.promotion,
                                move_3.en_passant,
                                move_3.left_white_castle_moved,
                                move_3.right_white_castle_moved,
                                move_3.left_black_castle_moved,
                                move_3.right_black_castle_moved);

                            #region Backup Check
                            for (int r = 0; r < 8; r++)
                            {
                                for (int c = 0; c < 8; c++)
                                {
                                    if (!(Virtual_Board.Position[r, c] == backup3[r, c]))
                                    {
                                        int oh_no = 0;
                                    }
                                }
                            }
                            #endregion

                            if (all_checkmate) break;
                        }

                        #endregion

                        Virtual_Board.UndoMove(
                            move_2.old_x,
                            move_2.old_y,
                            move_2.new_x,
                            move_2.new_y,
                            move_2.taken_piece,
                            move_2.promotion,
                            move_2.en_passant,
                            move_2.left_white_castle_moved,
                            move_2.right_white_castle_moved,
                            move_2.left_black_castle_moved,
                            move_2.right_black_castle_moved);

                        #region Backup Check
                        for (int r = 0; r < 8; r++)
                        {
                            for (int c = 0; c < 8; c++)
                            {
                                if (!(Virtual_Board.Position[r, c] == backup2[r, c]))
                                {
                                    int oh_no = 0;
                                }
                            }
                        }
                        #endregion

                        counter++;
                    }

                    Virtual_Board.UndoMove(candidate.old_x,
                        candidate.old_y,
                        candidate.new_x,
                        candidate.new_y,
                        candidate.taken_piece,
                        candidate.promotion,
                        candidate.en_passant,
                        candidate.left_white_castle_moved,
                        candidate.right_white_castle_moved,
                        candidate.left_black_castle_moved,
                        candidate.right_black_castle_moved);

                    if (all_checkmate)
                    {
                        return i;
                    }

                    Virtual_Board.ChangeTurn();
                }

                Virtual_Board.ChangeTurn();

                #region Backup Check
                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        if (!(Virtual_Board.Position[r, c] == backup1[r, c]))
                        {
                            int oh_no = 0;
                        }
                    }
                }
                #endregion
            }

            #endregion

            move_stats = new string[Current_Possible_Moves.Length];
            
            for (int i = 0; i < Current_Possible_Moves.Length; i++)
            {
                #region Backup
                string[,] backup1 = new string[8, 8];
                int backup2 = White_Piece_Length;
                int backup3 = Black_Piece_Length;

                // copy board
                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        backup1[r, c] = Virtual_Board.Position[r, c];
                    }
                }
                #endregion

                candidate = Virtual_Board.String_To_Move(Current_Possible_Moves[i]);

                Point[] prev_control_squares = Virtual_Board
                    .Attacked_Squares(candidate.old_x, candidate.old_y);

                move_stats[i] = "Move: " + candidate.move_notation + "\n";

                #region Move Middle Pawns

                if (Virtual_Board.Position[ai_pawn_row, 3] != ""
                        && Virtual_Board.Position[ai_pawn_row, 4] != ""
                        && candidate.old_y == ai_pawn_row
                        && (candidate.old_x == 3 || candidate.old_x == 4))
                {
                    Possible_Moves_Points[i] += move_middle_pawn_points;
                    move_stats[i] += "Gen Points - move middle pawn: " + move_middle_pawn_points.ToString() + "\n";
                }

                #endregion

                #region Open Bishop/ Queen Diagonals

                // ** change ** to accommodate bishops already developped

                for (int c = 3; c < 5; c++)
                {
                    if (Virtual_Board.Position[ai_pawn_row, c] != ""
                        && candidate.old_y == ai_pawn_row
                        && candidate.old_x == c)
                    {
                        Possible_Moves_Points[i] += (c / 2) * open_diagonal_points;
                        move_stats[i] += "Gen Points - open bishop diagonals: "
                            + ((c / 2) * open_diagonal_points).ToString() + "\n";
                    }
                }

                #endregion

                Virtual_Board.MakeMove(Current_Possible_Moves[i]);

                #region Change White_Piece / Black_Piece

                for (int j = 0; j < Black_Piece_Length; j++)
                {
                    if (ai_colour == "B"
                        && Black_Piece[j].Y == candidate.old_y
                        && Black_Piece[j].X == candidate.old_x)
                    {
                        Black_Piece[j].Y = candidate.new_y;
                        Black_Piece[j].X = candidate.new_x;
                    }
                    else if (ai_colour == "W"
                        && Virtual_Board.Position[Black_Piece[j].Y, Black_Piece[j].X] == "")
                    {
                        Point temp = new Point();
                        temp.X = Black_Piece[Black_Piece_Length - 1].X;
                        temp.Y = Black_Piece[Black_Piece_Length - 1].Y;

                        Black_Piece[Black_Piece_Length - 1].X = Black_Piece[j].X;
                        Black_Piece[Black_Piece_Length - 1].Y = Black_Piece[j].Y;

                        Black_Piece[j].X = temp.X;
                        Black_Piece[j].Y = temp.Y;

                        if (Black_Piece[j].Y == candidate.old_y
                        && Black_Piece[j].X == candidate.old_x)
                        {
                            Black_Piece[j].Y = candidate.new_y;
                            Black_Piece[j].X = candidate.new_x;
                        }

                        Black_Piece_Length--;
                    }
                }


                for (int j = 0; j < White_Piece_Length; j++)
                {
                    if (ai_colour == "W"
                        && White_Piece[j].Y == candidate.old_y
                        && White_Piece[j].X == candidate.old_x)
                    {
                        White_Piece[j].Y = candidate.new_y;
                        White_Piece[j].X = candidate.new_x;
                    }
                    else if (ai_colour == "B"
                        && Virtual_Board.Position[White_Piece[j].Y, White_Piece[j].X] == "")
                    {
                        Point temp = new Point();
                        temp.X = White_Piece[White_Piece_Length - 1].X;
                        temp.Y = White_Piece[White_Piece_Length - 1].Y;

                        White_Piece[White_Piece_Length - 1].X = White_Piece[j].X;
                        White_Piece[White_Piece_Length - 1].Y = White_Piece[j].Y;

                        White_Piece[j].X = temp.X;
                        White_Piece[j].Y = temp.Y;

                        if (White_Piece[j].Y == candidate.old_y
                        && White_Piece[j].X == candidate.old_x)
                        {
                            White_Piece[j].Y = candidate.new_y;
                            White_Piece[j].X = candidate.new_x;
                        }

                        White_Piece_Length--;
                    }
                }


                #endregion

                #region Change Field

                {
                    SetMatField();

                    int y = 0;
                    int x = 0;
                    Point[] attacked_squares = null;
                    string piece = null;
                    int points_num = 0;

                    // White
                    for (int k = 0; k < White_Piece_Length; k++)
                    {
                        y = White_Piece[k].Y;
                        x = White_Piece[k].X;

                        if (Virtual_Board.Position[y, x] != "") // <- ** shouldn't even be here
                        {
                            attacked_squares = Virtual_Board.Attacked_Squares(x, y);

                            piece = Virtual_Board.Position[y, x].Substring(6, 1);
                            points_num = PieceNum(piece);

                            for (int j = 0; j < attacked_squares.Length; j++)
                            {
                                White_Material_Field[attacked_squares[j].Y,
                                    attacked_squares[j].X] += points_num;
                            }
                        }
                    }

                    // Black
                    for (int k = 0; k < Black_Piece_Length; k++)
                    {
                        y = Black_Piece[k].Y;
                        x = Black_Piece[k].X;

                        if (Virtual_Board.Position[y, x] != "") // <- ** shouldn't even be here
                        {
                            attacked_squares = Virtual_Board.Attacked_Squares(x, y);

                            piece = Virtual_Board.Position[y, x].Substring(6, 1);

                            points_num = PieceNum(piece);

                            for (int j = 0; j < attacked_squares.Length; j++)
                            {
                                Black_Material_Field[attacked_squares[j].Y,
                                    attacked_squares[j].X] += points_num;
                            }
                        }
                    }
                }

                #endregion

                #region Avoid Checkmate

                bool into_checkmate = false;
                for (int j = 0; j < Virtual_Board.Possible_Moves.Length
                    && !into_checkmate; j++)
                {
                    move_2 = Virtual_Board.String_To_Move(Virtual_Board.Possible_Moves[j]);

                    Virtual_Board.MakeMove(Virtual_Board.Possible_Moves[j]);

                    if (Virtual_Board.Checkmate())
                    {
                        Possible_Moves_Points[i] -= 500000;
                        move_stats[i] += "Gen Points - into checkmate: " + (-500000).ToString() + "\n";
                        into_checkmate = true;
                    }

                    Virtual_Board.UndoMove(
                            move_2.old_x,
                            move_2.old_y,
                            move_2.new_x,
                            move_2.new_y,
                            move_2.taken_piece,
                            move_2.promotion,
                            move_2.en_passant,
                            move_2.left_white_castle_moved,
                            move_2.right_white_castle_moved,
                            move_2.left_black_castle_moved,
                            move_2.right_black_castle_moved);
                }

                #endregion

                #region Develop Piece

                if ((candidate.moved_piece == "B" || candidate.moved_piece == "N")
                    && candidate.old_y == ai_back_row)
                {
                    Possible_Moves_Points[i] += develop_points;
                    move_stats[i] += "Gen Points - develop piece: " + develop_points.ToString() + "\n";
                }

                #endregion

                #region Castle

                if (candidate.moved_piece == "K"
                    && (candidate.new_x - candidate.old_x == 2
                    || candidate.new_x - candidate.old_x == -2))
                {
                    Possible_Moves_Points[i] += castle_points;
                    move_stats[i] += "Gen Points - castle points: " + castle_points.ToString() + "\n";

                }

                #endregion

                #region Knight Rim

                if (candidate.moved_piece == "N"
                    && (candidate.new_x == 7
                    || candidate.new_x == 0
                    || candidate.new_y == 7
                    || candidate.new_y == 0))
                {
                    Possible_Moves_Points[i] += knight_rim_points;
                    move_stats[i] += "Gen Points - knight rim: " + knight_rim_points.ToString() + "\n";
                }

                #endregion

                Point[] control_squares = Virtual_Board
                    .Attacked_Squares(candidate.new_x, candidate.new_y);

                #region Centre Control

                for (int j = 0; j < control_squares.Length; j++)
                {
                    if ((control_squares[j].X == 3
                        || control_squares[j].X == 4)
                        && (control_squares[j].Y == 3
                        || control_squares[j].Y == 4))
                    {
                        Possible_Moves_Points[i] += centre_control_points;
                        move_stats[i] += "Gen Points - centre control points: " + centre_control_points.ToString() + "\n";
                    }
                }

                for (int j = 0; j < prev_control_squares.Length; j++)
                {
                    if ((prev_control_squares[j].X == 3
                        || prev_control_squares[j].X == 4)
                        && (prev_control_squares[j].Y == 3
                        || prev_control_squares[j].Y == 4))
                    {
                        Possible_Moves_Points[i] -= centre_control_points;
                        move_stats[i] += "Gen Points - centre control points: " + (-centre_control_points).ToString() + "\n";
                    }
                }

                #endregion

                #region Material Points
                {
                    if (candidate.taken_piece == "")
                    {
                        Possible_Moves_Material[i] = 0;
                        move_stats[i] += "Material Points: = 0" + "\n";
                    }
                    else
                    {
                        Possible_Moves_Material[i] = PieceWorth(candidate.taken_piece.Substring(6, 1));
                        move_stats[i] += "Material Points: "
                            + PieceWorth(candidate.taken_piece.Substring(6, 1)).ToString() + "\n";
                    }

                    int x = 0;
                    int y = 0;
                    int piece_worth = 0;
                    int piece_num = 0;

                    if (ai_colour == "W")
                    {
                        // if AI is white

                        for (int j = 0; j < White_Piece_Length; j++)
                        {
                            x = White_Piece[j].X;
                            y = White_Piece[j].Y;

                            if (Virtual_Board.Position[y, x] != "") // <- ** shouldn't even be here
                            {
                                piece_worth = PieceWorth(Virtual_Board.Position[y, x].Substring(6, 1));
                                piece_num = PieceNum(Virtual_Board.Position[y, x].Substring(6, 1));

                                int stuff1 = Black_Material_Field[y, x];

                                if (Black_Material_Field[y, x] != 0
                                    && White_Material_Field[y, x] == 0)
                                {
                                    Possible_Moves_Material[i] -= piece_worth;
                                    move_stats[i] += "Material Points: " + (-piece_worth).ToString() + "\n";
                                }
                                else if (Black_Material_Field[y, x] % piece_num > 0)
                                {
                                    int stuff = PieceNumToPieceWorth(RightNonZero(
                                        Black_Material_Field[y, x]));
                                    Possible_Moves_Material[i] -=
                                        piece_worth - PieceNumToPieceWorth(RightNonZero(
                                        Black_Material_Field[y, x]));
                                    move_stats[i] += "Material Points: "
                                        + (-(piece_worth - PieceNumToPieceWorth(RightNonZero(
                                        Black_Material_Field[y, x])))).ToString() + "\n";
                                }
                                else if (DigitSum(White_Material_Field[y, x]) <
                                    DigitSum(Black_Material_Field[y, x]))
                                {
                                    Possible_Moves_Material[i] -= piece_worth;
                                    move_stats[i] += "Material Points: " + (-piece_worth).ToString() + "\n";
                                }
                            }
                        }
                    }
                    else
                    {
                        // AI is black

                        for (int j = 0; j < Black_Piece_Length; j++)
                        {
                            x = Black_Piece[j].X;
                            y = Black_Piece[j].Y;

                            if (Virtual_Board.Position[y, x] != "") // <- ** shouldn't even be here
                            {
                                piece_worth = PieceWorth(Virtual_Board.Position[y, x].Substring(6, 1));
                                piece_num = PieceNum(Virtual_Board.Position[y, x].Substring(6, 1));

                                if (Black_Material_Field[y, x] == 0
                                    && White_Material_Field[y, x] != 0)
                                {
                                    Possible_Moves_Material[i] -= piece_worth;
                                    move_stats[i] += "Material Points: " + (-piece_worth).ToString() + "\n";
                                }

                                else if (White_Material_Field[y, x] % piece_num > 0)
                                {
                                    Possible_Moves_Material[i] -=
                                        piece_worth - PieceNumToPieceWorth(RightNonZero(
                                        White_Material_Field[y, x]));
                                    move_stats[i] += "Material Points: "
                                        + (-(piece_worth - PieceNumToPieceWorth(RightNonZero(
                                        White_Material_Field[y, x])))).ToString() + "\n";
                                }
                                else if (DigitSum(Black_Material_Field[y, x]) <
                                    DigitSum(White_Material_Field[y, x]))
                                {
                                    Possible_Moves_Material[i] -= piece_worth;
                                    move_stats[i] += "Material Points: " + (-piece_worth).ToString() + "\n";
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Tempo & Fork

                {
                    int tempo_result = Virtual_Board.Tempo_Fork(candidate, control_squares, White_Material_Field, Black_Material_Field);

                    if (tempo_result > -1)
                    {
                        Possible_Moves_Points[i] += tempo_points;
                        move_stats[i] += "Gen Points - tempo points: " + tempo_points.ToString() + "\n";

                        if (tempo_result > 0 && Possible_Moves_Material[i] >= 0)
                        {
                            move_stats[i] += "Material Points: " + tempo_result.ToString() + "\n";
                        }
                    }
                }

                #endregion

                #region Fianchetto

                // left
                if (Virtual_Board.Position[ai_back_row, 2] != ""
                    && Virtual_Board.Position[ai_back_row, 2].Substring(6, 1) == "B"
                    && candidate.moved_piece == "P"
                    && candidate.old_x == 1
                    && candidate.old_y == ai_pawn_row
                    && candidate.new_x == 1
                    && ((candidate.new_y - candidate.old_y) * (candidate.new_y - candidate.old_y)) == 1)
                {
                    Possible_Moves_Points[i] += fianchetto_points;
                    move_stats[i] += "Gen Points - fianchetto points: " + fianchetto_points.ToString() + "\n";
                }

                // right
                if (Virtual_Board.Position[ai_back_row, 5] != ""
                    && Virtual_Board.Position[ai_back_row, 5].Substring(6, 1) == "B"
                    && candidate.moved_piece == "P"
                    && candidate.old_x == 6
                    && candidate.old_y == ai_pawn_row
                    && candidate.new_x == 6
                    && ((candidate.new_y - candidate.old_y) * (candidate.new_y - candidate.old_y)) == 1)
                {
                    Possible_Moves_Points[i] += fianchetto_points;
                    move_stats[i] += "Gen Points - fianchetto points: " + fianchetto_points.ToString() + "\n";
                }

                #endregion

                #region Rook to Opponent Pawn Ranks

                if (candidate.moved_piece == "R" && candidate.new_y == 8 - ai_pawn_row)
                {
                    Possible_Moves_Points[i] += rook_to_pawn_rank_points;
                    move_stats[i] += "Gen Points - rook to pawn rank points: " + rook_to_pawn_rank_points.ToString() + "\n";
                }

                #endregion

                #region Pin
                {
                    int field_point = ai_colour == "W" ?
                                            RightNonZero(Black_Material_Field[candidate.new_y, candidate.new_x]) :
                                            RightNonZero(White_Material_Field[candidate.new_y, candidate.new_x]);

                    int pin_result = Virtual_Board.Pin_Move(candidate, field_point);

                    if (pin_result > -1)
                    {
                        Possible_Moves_Points[i] += pin_points;
                        move_stats[i] += "Gen Points - pin points: " + pin_points.ToString() + "\n";

                        if (pin_result > 0)
                        {
                            Possible_Moves_Material[i] += pin_result;
                            move_stats[i] += "Material Points: " + pin_result.ToString() + "\n";
                        }
                    }


                }
                #endregion

                #region Skewer

                {
                    int field_point = ai_colour == "W" ?
                                        RightNonZero(Black_Material_Field[candidate.new_y, candidate.new_x]) :
                                        RightNonZero(White_Material_Field[candidate.new_y, candidate.new_x]);

                    int skewer_result = Virtual_Board.Pin_Move(candidate, field_point);

                    if (skewer_result > 0)
                    {
                        Possible_Moves_Material[i] += skewer_result;
                        move_stats[i] += "Material Points: " + skewer_result.ToString() + "\n";
                    }

                }

                #endregion

                #region Opponent Fork

                {
                    string opp_candidate_str;
                    ChessMove opp_candidate;
                    int opp_fork;
                    int max_opp_fork = 0;
                    string[,] backup4 = new string[8, 8];

                    for (int j = 0; j < Virtual_Board.Possible_Moves.Length; j++)
                    {
                        opp_candidate_str = Virtual_Board.Possible_Moves[j];
                        opp_candidate = Virtual_Board.String_To_Move(opp_candidate_str);
                        
                        #region Backup Set Up
                        // copy board
                        for (int r = 0; r < 8; r++)
                        {
                            for (int c = 0; c < 8; c++)
                            {
                                backup4[r, c] = Virtual_Board.Position[r, c];
                            }
                        }
                        #endregion

                        Virtual_Board.MakeMove(opp_candidate_str);

                        opp_fork = Virtual_Board.Tempo_Fork(opp_candidate, 
                            Virtual_Board.Attacked_Squares(opp_candidate.new_x, opp_candidate.new_y), 
                            Virtual_Board.GetMaterialField(true),
                            Virtual_Board.GetMaterialField(false));

                        if (opp_fork > max_opp_fork)
                        {
                            max_opp_fork = opp_fork;
                        }

                        Virtual_Board.UndoMove(opp_candidate.old_x,
                            opp_candidate.old_y,
                            opp_candidate.new_x,
                            opp_candidate.new_y,
                            opp_candidate.taken_piece,
                            opp_candidate.promotion,
                            opp_candidate.en_passant,
                            opp_candidate.left_white_castle_moved,
                            opp_candidate.right_white_castle_moved,
                            opp_candidate.left_black_castle_moved,
                            opp_candidate.right_black_castle_moved);


                        #region Backup Check
                        for (int r = 0; r < 8; r++)
                        {
                            for (int c = 0; c < 8; c++)
                            {
                                if (!(Virtual_Board.Position[r, c] == backup4[r, c]))
                                {
                                    int oh_no = 0;
                                }
                            }
                        }
                        #endregion
                    }

                    Possible_Moves_Material[i] -= max_opp_fork;
                }

                #endregion

                #region Queen-King Checkmate

                // check if opponent has only pawns and possibly a bishop/knight
                // check if all pawns have opponents
                // restrict king moves to 2
                // move king so it is 2 away
                // king & queen vs king & bishop/knight

                {
                    bool queen_king_checkmate = true;
                    int three_point_piece_count = 0;

                    Point[] pieces = ai_colour == "W" ? Black_Piece : White_Piece;
                    int start_r;
                    int end_r;
                    string current_piece;
                    int current_piece_worth;
                    // ai_pawn_direction

                    for (int j = 0; j < pieces.Length; j++)
                    {
                        queen_king_checkmate = false;
                        
                        current_piece = Virtual_Board.Position[pieces[j].Y, pieces[j].X].Substring(6, 1);
                        current_piece_worth = PieceWorth(current_piece);

                        if (current_piece_worth == 3)
                        {
                            three_point_piece_count++;
                        }

                        if (current_piece == "P") {
                            start_r = pieces[j].Y;
                            end_r = ai_colour == "W" ? 0 : 7;

                            if ((end_r - start_r) * ai_pawn_direction < 0)
                            {
                                int oh_no = 0;
                            }

                            for (int r = start_r; r < end_r; r += ai_pawn_direction)
                            {
                                if (Virtual_Board.Position[r, pieces[j].X] != "")
                                {
                                    queen_king_checkmate = true;
                                    break;
                                }
                            }                           
                            
                            if (queen_king_checkmate == false)
                            {
                                break;
                            }
                            else if (current_piece_worth > 3) {
                                queen_king_checkmate = false;
                                break;
                            }
                        }
                    }

                    if (queen_king_checkmate && three_point_piece_count < 2)
                    {
                        int king_space = -1;
                        int king_distance = -1;


                    }
                }

                

                #endregion

                #region Rooks to Open / Semi-Open Files

                if (candidate.moved_piece == "R")
                {
                    bool no_pawns = true;
                    bool missing_own_pawn = true;

                    int start_r = 6 - (ai_pawn_direction + 1) * 5 / 2;
                    int end_r = 7 - start_r;


                    // -1 -> 6: 6 - (x + 1) * 5 / 2
                    // 1 -> 1: 6 - (x + 1) * 5 / 2

                    // new position
                    for (int r = start_r; r != end_r; r += ai_pawn_direction)
                    {
                        if (Virtual_Board.Position[r, candidate.new_x] != ""
                            && Virtual_Board.Position[r, candidate.new_x].Substring(6, 1) == "P")
                        {
                            no_pawns = false;

                            if (Virtual_Board.Position[r, candidate.new_x].Substring(0, 1) == ai_colour)
                            {
                                missing_own_pawn = false;
                            }
                        }
                    }

                    if (no_pawns)
                    {
                        Possible_Moves_Points[i] += rook_to_open_file_points;
                    }
                    else if (missing_own_pawn)
                    {
                        Possible_Moves_Points[i] += rook_to_semi_open_file_points;
                    }

                    // old position
                    no_pawns = true;
                    missing_own_pawn = true;

                    for (int r = start_r; r != end_r; r += ai_pawn_direction)
                    {
                        if (Virtual_Board.Position[r, candidate.old_x] != ""
                            && Virtual_Board.Position[r, candidate.old_x].Substring(6, 1) == "P")
                        {
                            no_pawns = false;

                            if (Virtual_Board.Position[r, candidate.old_x].Substring(0, 1) == ai_colour)
                            {
                                missing_own_pawn = false;
                            }
                        }
                    }

                    if (no_pawns)
                    {
                        Possible_Moves_Points[i] -= rook_to_open_file_points;
                    }
                    else if (missing_own_pawn)
                    {
                        Possible_Moves_Points[i] -= rook_to_semi_open_file_points;
                    }

                }

                #endregion

                Virtual_Board.UndoMove(candidate.old_x,
                    candidate.old_y,
                    candidate.new_x,
                    candidate.new_y,
                    candidate.taken_piece,
                    candidate.promotion,
                    candidate.en_passant,
                    candidate.left_white_castle_moved,
                    candidate.right_white_castle_moved,
                    candidate.left_black_castle_moved,
                    candidate.right_black_castle_moved);

                #region Change White_Piece / Black_Piece

                if (ai_colour == "B")
                {
                    for (int j = 0; j < Black_Piece_Length; j++)
                    {
                        if (Black_Piece[j].Y == candidate.new_y
                            && Black_Piece[j].X == candidate.new_x)
                        {
                            Black_Piece[j].Y = candidate.old_y;
                            Black_Piece[j].X = candidate.old_x;
                        }
                    }

                    if (White_Piece.Length > White_Piece_Length
                        && Virtual_Board.Position[White_Piece[White_Piece_Length].Y,
                        White_Piece[White_Piece_Length].X] != ""
                        && Virtual_Board.Position[Black_Piece[Black_Piece_Length].Y,
                        Black_Piece[Black_Piece_Length].X].Substring(0, 1) == "W")
                    {
                        White_Piece_Length++;
                    }
                }
                else
                {
                    for (int j = 0; j < White_Piece_Length; j++)
                    {
                        if (White_Piece[j].Y == candidate.new_y
                            && White_Piece[j].X == candidate.new_x)
                        {
                            White_Piece[j].Y = candidate.old_y;
                            White_Piece[j].X = candidate.old_x;
                        }
                    }

                    if (Black_Piece.Length > Black_Piece_Length
                        && Virtual_Board.Position[Black_Piece[Black_Piece_Length].Y,
                        Black_Piece[Black_Piece_Length].X] != ""
                        && Virtual_Board.Position[Black_Piece[Black_Piece_Length].Y,
                        Black_Piece[Black_Piece_Length].X].Substring(0, 1) == "B")
                    {
                        Black_Piece_Length++;
                    }
                }

                #endregion

                #region Backup Check
                for (int r = 0; r < 8; r++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        if (!(Virtual_Board.Position[r, c] == backup1[r, c]))
                        {
                            int oh_no = 0;
                        }
                    }
                }

                if (backup2 != White_Piece_Length)
                {
                    int oh_no = 0;
                }
                if (backup3 != Black_Piece_Length)
                {
                    int oh_no = 0;
                }
                #endregion
            }

            #region Choose Max Score

            if (chosen_move == -1)
            {
                max_material_score = Possible_Moves_Material[0];
                max_score = Possible_Moves_Points[0];
                max_occur = 1;

                for (int i = 1; i < Current_Possible_Moves.Length; i++)
                {
                    if (Possible_Moves_Material[i] == max_material_score
                        && Possible_Moves_Points[i] == max_score)
                    {
                        max_occur++;
                    }
                    else if (Possible_Moves_Material[i] > max_material_score)
                    {
                        max_material_score = Possible_Moves_Material[i];
                        max_score = Possible_Moves_Points[i];
                        max_occur = 1;
                    }

                    if (Possible_Moves_Material[i] == max_material_score
                        && Possible_Moves_Points[i] > max_score)
                    {
                        max_score = Possible_Moves_Points[i];
                        max_occur = 1;
                    }
                }

                Random rnd = new Random();
                max_occur = rnd.Next(max_occur);

                for (int i = 0; i < Current_Possible_Moves.Length; i++)
                {
                    if (max_occur == 0 && Possible_Moves_Points[i] == max_score
                        && Possible_Moves_Material[i] == max_material_score)
                    {
                        return i;
                    }
                    else if (Possible_Moves_Points[i] == max_score
                        && Possible_Moves_Material[i] == max_material_score)
                    {
                        max_occur--;
                    }
                }

                int stuff = 0;
            }

            #endregion

            return -1;

            // (DONE) opening book moves
            // (DONE) checkmate moves
            // (DONE) avoid checkmate

            // (DONE) develop pieces
            // (DONE) castle move
            // (DONE) avoid knight on edge

            // (DONE) control the centre            
            // (DONE) tempo
            // (DONE) fork moves

            // (DONE) fianchetto
            // (DONE) rook to pawn rank

            // (DONE) pin moves
            // (DONE) skewer moves

            // (DONE) move rooks on open/semi-open files

            // (DONE) Avoid giving opponent fork moves
            
            // * checkmate with Queen and King
            // * checkmate with King and Rook
            
            
            // * prevent opponent promotion

            // material to do:
            // * see batteries
            // * take pinned pieces into account

            // incorporate blocking with skewers

            // incorporate discovered pins

            // Rules of thumb:
            // * don't move the kingside pawns
            // * undermining
            // * pawn structure

            // * Avoid fork
            // * find mate in 2n + 1 moves
            // * avoid mate in 2n + 1 moves
            // * avoid double pawns
            // * avoid isolated pawns
            // * attack pinned piece
            // * bishop sac
            // * windmill
            // * deflection
            // * decoy
            // * x-ray
            // * investigate check moves
            // * back row mate
            // * prevent pawn promotion
            // * push passed pawns
            // * attack piece that cannot be defended
            // * poisoned pawn
            // * look for check moves
            // * trap moves



            // checkmate with bishop and knight?
            // checkmate with two bishops?

            // average Joe:
            // * somtimes sees tactics
            // * usually defends against checkmate
            // * takes undefended material
            // * sometimes moves kingside pawns
            // * basic material points system in mind
            // * falls for traps  (espcially if his move sets it up)

        }

        #endregion
    }
}
