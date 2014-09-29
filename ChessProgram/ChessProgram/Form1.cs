using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
// using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace ChessProgram
{
    public partial class Form1 : Form
    {
        ChessGame game = new ChessGame();
        string prev_click = "";
        public string[] chess_pieces = { "♔", "♕", "♖", "♗", "♘", "♙", "♚", 
                                    "♛", "♜", "♝", "♞", "♟"};

        string version = "v13";



        public bool opening_input = false;

        public Form1()
        {
            InitializeComponent();

            RefreshForm();
        }

        public Rectangle CoorToRectangle(int x1, int y1, int x2, int y2)
        {
            Rectangle result = new Rectangle();

            result.X = x1;
            result.Y = y1;
            result.Width = x2 - x1 + 1;
            result.Height = y2 - y1 + 1;

            return result;
        }

        public void DrawString(string text, int x, int y, int rotation)
        {
            float xFloat = Convert.ToSingle(x);
            float yFloat = Convert.ToSingle(y);

            double oldAngleRad = Math.Asin(-1 * yFloat / Math.Sqrt(xFloat * xFloat + yFloat * yFloat));
            // if (oldAngleRad < 0) oldAngleRad += 2 * Math.PI;
            double newAngleRad = rotation / 360f * 2 * Math.PI;

            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 45);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();

            formGraphics.RotateTransform(rotation);

            float newX = Convert.ToSingle(Math.Sqrt(Convert.ToDouble(x * x + y * y)) * Math.Cos(newAngleRad + oldAngleRad));
            float newY = -1 * Convert.ToSingle(Math.Sqrt(Convert.ToDouble(x * x + y * y)) * Math.Sin(newAngleRad + oldAngleRad));

            formGraphics.DrawString(text, drawFont, drawBrush, newX, newY, drawFormat);

            formGraphics.ResetTransform();

            drawFont.Dispose();
            drawBrush.Dispose();
            formGraphics.Dispose();

            Application.DoEvents();
        }

        public void DrawRectangle(Rectangle rec, Color colour)
        {
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(colour);
            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            formGraphics.FillRectangle(myBrush, rec);
            myBrush.Dispose();
            formGraphics.Dispose();
        }

        public void RefreshForm()
        {
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            formGraphics.FillRectangle(myBrush, new Rectangle(0, 0, 1255, 712));
            myBrush.Dispose();
            formGraphics.Dispose();
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            System.Drawing.Pen myPen;
            myPen = new System.Drawing.Pen(System.Drawing.Color.Black);
            System.Drawing.Graphics formGraphics = this.CreateGraphics();
            formGraphics.DrawLine(myPen, x1, y1, x2, y2);
            myPen.Dispose();
            formGraphics.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string player_col = "W";
            Random rnd = new Random();
            if (rnd.Next(2) == 0)
            {
                player_col = "B";
            }

            game.NewGame(player_col, true);

            string[] lines = new string[2] { "Italian-Koltanowski Gambit", "Damiano Defence" };
            game.Opponent.ChangeOpeningBook(lines);

            UpdateBoard();
            
            if (game.player_turn == false)
            {
                game.AIMove();
                EndGame();
            }
        }

        public string ChessTxtToChar(string text)
        {
            string result = "";
            int adder = 0;

            if (text.Length > 0)
            {
                if (text.Substring(0, 5) == "Black")
                {
                    adder = 6;
                }

                if (text.Substring(6) == "K")
                {
                    adder += 0;
                }

                else if (text.Substring(6) == "Q")
                {
                    adder += 1;
                }
                else if (text.Substring(6) == "R")
                {
                    adder += 2;
                }
                else if (text.Substring(6) == "B")
                {
                    adder += 3;
                }
                else if (text.Substring(6) == "N")
                {
                    adder += 4;
                }
                else
                {
                    adder += 5;
                }

                result = chess_pieces[adder];
            }

            return result;
        }

        void UpdateBoard()
        {
            int x = 10;
            int y = 10;
            int width = 640;
            int height = 640;

            int side = 1; // 1 means player is white
            if (game.player_colour == "B")
            {
                side = 0;
            }

            RefreshForm();

            #region Draw Board

            // Outline
            for (int i = 0; i <= 8; i++)
            {
                DrawLine(x + i * (width / 8), y, x + i * (width / 8), y + height);
                DrawLine(x, y + i * (height / 8), x + width, y + i * (height / 8));
            }

            Rectangle arg;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if ((c + r) % 2 == 1)
                    {
                        arg = CoorToRectangle(x + c * (width / 8), y + r * (height / 8),
                            x + (c + 1) * (width / 8) - 1, y + (r + 1) * (height / 8) - 1);
                        DrawRectangle(arg, Color.Brown);
                    }
                }
            }

            #endregion

            #region Draw Pieces

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (side == 1)
                    {
                        DrawString(ChessTxtToChar(game.Board.Position[r, c]), 10 + c * (width / 8),
                            10 + r * (height / 8) + 10, 0);
                    }
                    else
                    {
                        DrawString(ChessTxtToChar(game.Board.Position[r, c]), 10 + (7 - c) * (width / 8),
                            10 + (7 - r) * (height / 8) + 10, 0);
                    }
                }
            }

            #endregion
        }

        public void EndGame()
        {
            UpdateBoard();

            if (game.Board.Checkmate())
            {
                game.in_game = false;

                if (game.Board.Turn == "W")
                {
                    MessageBox.Show("Black wins!");
                }
                else
                {
                    MessageBox.Show("White wins!");
                }

                Database db = new Database("Games");

                if (db.tag_length == 0)
                {
                    db.AddTag("White Player");
                    db.AddTag("Black Player");
                    db.AddTag("Chess Notation");
                    db.AddTag("Comments");
                }

                string[] entry = new string[4];

                if (game.player_colour == "W")
                {
                    entry[0] = "Henri"; // *****
                    entry[1] = "Computer " + version;
                }
                else
                {
                    entry[1] = "Henri"; // *****
                    entry[0] = "Computer " + version;
                }
                entry[2] = game.MoveListToNotation();
                entry[3] = "";

                db.AddEntry(entry);
            }
            else if (game.Board.Stalemate())
            {
                game.in_game = false;

                MessageBox.Show("Stalemate");
            }
        }

        public void PlayerMove(string move)
        {
            // input opening move
            if (opening_input)
            {
                Database db = new Database("Opening Book - " + cbOpeningBookName.Text);

                if (db.tag_length == 0)
                {
                    db.AddTag("turn");
                    db.AddTag("move_count");
                    db.AddTag("piece_count");
                    db.AddTag("pieces");
                    db.AddTag("move");
                }

                string[] entry = new string[5];
                entry[0] = game.Board.Turn;
                entry[1] = (game.move_count + 1).ToString();
                entry[2] = (game.Board.piece_count).ToString();
                entry[3] = game.Board.BoardToString();
                entry[4] = move;

                db.AddEntry(entry);
            }
            
            game.Move(move);
            game.player_turn = !game.ai_opponent;

            EndGame();

            if (game.ai_opponent && game.in_game)
            {
                game.AIMove();

                rtbMovePoints.Text = "Material Points:\n" + game.Opponent.max_material_score.ToString() + "\n";
                rtbMovePoints.Text += "General Points:\n" + game.Opponent.max_score.ToString() + "\n";

                rtbMoveStats.Text = "";
                if (game.Opponent.move_stats != null)
                {
                    for (int j = 0; j < game.Opponent.move_stats.Length; j++)
                    {
                        rtbMoveStats.Text += game.Opponent.move_stats[j] + "\n";
                    }
                }

                EndGame();
            }
        }

        private void Form1_MouseClick(Object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;

            int board_x = 10;
            int board_y = 10;
            int width = 640;
            int height = 640;

            int r1;
            int c1;

            string prev_spot;
            string click;
            string move;

            Rectangle rec;

            if (game.in_game
                && x >= board_x && x <= board_x + width
                && y >= board_y && y < board_y + height
                && game.player_turn)
            {
                r1 = ((y - board_y) / 80);
                c1 = ((x - board_x) / 80);

                click = "(" + r1.ToString() + " "
                        + c1.ToString() + ")";

                if (prev_click == "")
                {
                    prev_click = click;

                    UpdateBoard();
                    // outline
                    // top
                    rec = new Rectangle(board_x + ((x - board_x) / 80) * 80,
                        board_y + ((y - board_y) / 80) * 80,
                        80, 10);
                    DrawRectangle(rec, Color.Yellow);
                    // bottom
                    rec = new Rectangle(board_x + ((x - board_x) / 80) * 80,
                        board_y + ((y - board_y) / 80 + 1) * 80 - 10,
                        80, 10);
                    DrawRectangle(rec, Color.Yellow);
                    // left
                    rec = new Rectangle(board_x + ((x - board_x) / 80) * 80,
                        board_y + ((y - board_y) / 80) * 80,
                        10, 80);
                    DrawRectangle(rec, Color.Yellow);
                    // right
                    rec = new Rectangle(board_x + ((x - board_x) / 80 + 1) * 80 - 10,
                        board_y + ((y - board_y) / 80) * 80,
                        10, 80);
                    DrawRectangle(rec, Color.Yellow);

                }
                else
                {
                    move = prev_click + " -> " + click;

                    // reverse move
                    if (game.player_colour == "B")
                    {
                        move = "(" + (7 - Convert.ToInt32(prev_click.Substring(1, 1))).ToString()
                            + " " + (7 - Convert.ToInt32(prev_click.Substring(3, 1))).ToString()
                            + ") -> (" + (7 - Convert.ToInt32(click.Substring(1, 1))).ToString()
                            + " " + (7 - Convert.ToInt32(click.Substring(3, 1))).ToString() + ")";
                    }

                    if (game.Board.PossibleMove(move))
                    {
                        // promotion
                        prev_spot = game.Board.Position[Convert.ToInt32(move.Substring(1, 1)),
                            Convert.ToInt32(move.Substring(3, 1))];

                        if ((r1 == 0 || r1 == 7)
                            && prev_spot.Length > 0
                            && prev_spot.Substring(6) == "P")
                        {
                            DialogResult result;
                            result = MessageBox.Show("Promote to a Queen?",
                                "Promotion",
                                MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                move += " -> Q";
                            }
                            else
                            {
                                result = MessageBox.Show("Promote to a Rook?",
                                "Promotion",
                                MessageBoxButtons.YesNo);
                                if (result == DialogResult.Yes)
                                {
                                    move += " -> R";
                                }
                                else
                                {
                                    result = MessageBox.Show("Promote to a Bishop?",
                                "Promotion",
                                MessageBoxButtons.YesNo);
                                    if (result == DialogResult.Yes)
                                    {
                                        move += " -> B";
                                    }
                                    else
                                    {
                                        move += " -> N";
                                    }
                                }
                            }
                        }

                        PlayerMove(move);
                    }
                    UpdateBoard();
                    prev_click = "";
                }
            }

            Application.DoEvents();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            game.NewGame("W", false);
            UpdateBoard();
        }

        private void btnUndoMove_Click(object sender, EventArgs e)
        {
            if (game.move_count > 0)
            {
                if (game.ai_opponent)
                {
                    game.Undo_Move();
                    game.Undo_Move();
                }
                else
                {
                    game.Undo_Move();
                    game.player_turn = !game.ai_opponent;
                }

                UpdateBoard();
            }
        }

        private void btnAddOpeningMove_Click(object sender, EventArgs e)
        {
            if (!(game.in_game))
            {
                return;
            }
            
            opening_input = !opening_input;

            if (opening_input)
            {
                btnAddOpeningMove.BackColor = Color.LightGreen;
            }
            else
            {
                btnAddOpeningMove.BackColor = Color.LightGray;
            }
        }

        // TO DO:
        // * king's gambit
        // * evan's gambit
        // * scotch gambit

        // * ponziani
        // * queen's gambit
        // * italian game
        // * ruy lopez
        // * f6 variation
        // * budapest gambit

        // Highest Chess Traps in Black Opening

        // dirty chess tricks 1
        // for black using Owen's Defence
        // dirty chess tricks 2
        // for black against Queen's pawn (Veresov Attack)
        // dirty chess tricks 3
        // for white (Tennison Gambit)
        // dirty chess tricks 4
        // for white (caro-kann)
        // dirty chess tricks 5
        // for white (philidor defence)
        // dirty chess tricks 6
        // for white (max lange attack (like scotch gambit))
        // dirty chess tricks 7
        // for white (morphy attack (like scotch gambit))
        // dirty chess tricks 8
        // for white (Italian-Koltanowski Gambit)


        // AI thinks for 2 seconds (thinks for 4 seconds when it sees a blunder
        // can take back move before AI makes a move

        



        // added: 
        // * piece_count
        // * fixed en passant var bug
    }
}
