using System;
using System.Drawing;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;

namespace Inequality
{
    class Simulation
    {
        private int m_width = 640;
        private int m_height = 480;

        private Surface m_video;

        private Person[] m_people;

        private int m_numberOfPeople = 20;

        private int m_startingMoney = 100;

        private Random m_random;

        private int m_elapsed;

        private int m_delay = 10;

        private int m_cycles = 0;

        private History m_history;

        private int m_scale = 4;

        private int m_peakDelay = 5;

        private int m_peakSpeed = 2;

        private int[] m_peakValues;

        private int[] m_peakDelays;

        private bool m_allowNegativeMoney = false;

        public Simulation()
        {
            m_video = Video.SetVideoMode(640, 480, false, false, false);

            m_people = new Person[m_numberOfPeople];

            m_random = new Random();

            m_peakValues = new int[m_numberOfPeople];
            m_peakDelays = new int[m_numberOfPeople];

            for (int i = 0; i < m_numberOfPeople; i++)
            {
                int r = m_random.Next(0, 256);
                int g = m_random.Next(0, 256);
                int b = m_random.Next(0, 256);
                m_people[i] = new Person(Color.FromArgb(r,g,b), m_startingMoney);

                m_peakValues[i] = 0;
                m_peakDelays[i] = 0;
            }

            m_elapsed = 0;

            m_history = new History(m_numberOfPeople, m_width);

            Events.Quit += Events_Quit;
            Events.Tick += Events_Tick;
        }

        private void Events_Tick(object sender, TickEventArgs e)
        {
            m_video.Fill(Color.Black);

            m_elapsed += e.TicksElapsed;
            if (m_elapsed > m_delay)
            {
                m_elapsed -= m_delay;

                // simulate

                simulate();

                // do updates after all simulation is complete

                for (int i = 0; i < m_numberOfPeople; i++)
                {
                    // update peaks

                    updatePeak(i);

                    // update history for person

                    m_history.Push(i, m_people[i].Money);
                }
            }

            int width = m_width / m_numberOfPeople;
            int scale = m_height / (m_startingMoney * m_scale);

            SdlDotNet.Graphics.Font font = new SdlDotNet.Graphics.Font("C:\\Windows\\Fonts\\ARIAL.TTF", 12);

            // draw bars

            for (int i = 0; i < m_numberOfPeople; i++)
            {
                // draw bar

                m_video.Draw(new Box(
                    (short)(width * i), 0,
                    (short)((width * i) + width-1), (short)(scale * m_people[i].Money)),
                    m_people[i].Color, false, true);

                // draw value

                m_video.Blit(font.Render(m_people[i].Money.ToString(), Color.White),
                    new Point((width * i) + 2, 10));

                // draw peak hold

                m_video.Draw(new Line(
                    (short)(width * i), (short)(scale * m_peakValues[i]),
                    (short)((width * i) + width - 1), (short)(scale * m_peakValues[i])
                    ), Color.White, false, true);

                // draw lines

                for (int j = 0; j < m_width; j++)
                {
                    m_video.Draw(new Point(j, m_height - (scale * m_history.Get(i, j))), m_people[i].Color);
                }
            }

            // update display

            m_video.Update();
        }

        private void simulate()
        {
            m_cycles += 1;
            
            for (int i = 0; i < m_numberOfPeople; i++)
            {
                Person a = m_people[i];

                if (a.Money > 0 || m_allowNegativeMoney)
                {
                    Person b = getRandomPerson(i);

                    a.Money -= 1;
                    b.Money += 1;
                }
            }

            Video.WindowCaption = string.Format("Cycles: {0}", m_cycles);
        }

        private void updatePeak(int i)
        {
            if (m_peakValues[i] <= m_people[i].Money)
            {
                // set wait time
                m_peakDelays[i] = m_peakDelay;

                // push peak to new max
                m_peakValues[i] = m_people[i].Money;
            }
            else
            {
                if (m_peakDelays[i] > 0)
                {
                    // pause before moving, decrement wait time
                    m_peakDelays[i] -= 1;
                }
                else
                {
                    // pause has timed out, decrement value
                    m_peakValues[i] -= 1;

                    // set new wait time
                    m_peakDelays[i] = m_peakSpeed;
                }
            }
        }

        private Person getRandomPerson(int not)
        {
            int selectedId = not;
            do
            {
                selectedId = m_random.Next(0, m_numberOfPeople);
            } while (selectedId == not);

            return m_people[selectedId];
        }

        private void Events_Quit(object sender, QuitEventArgs e)
        {
            Events.QuitApplication();
        }

        public void Run()
        {
            Events.Run();
        }
    }
}
