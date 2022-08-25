﻿using Skill_PMS.Controller;
using Skill_PMS.Data;
using Skill_PMS.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Skill_PMS.UI_WinForm.Production.Designer
{
    public partial class Dashboard : Form
    {
        private readonly Common _common = new Common();
        private SkillContext _db = new SkillContext();
        public static User User { get; set; }
        public Performance Performance = new Performance();

        public Dashboard()
        {
            InitializeComponent();
            Tbc_Designer.SelectedIndex = 1;
        }

        private static Dashboard _instance;
        public static Dashboard GetInstance()
        {
            if (_instance == null || _instance.IsDisposed)
                _instance = new Dashboard();
            else
                _instance.BringToFront();
            return _instance;
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            this.Text = @"Dashboard - " + User.Full_Name;
            Tmr_Count_Dashboard.Start();
            Check_New_Job();
            Check_Data();
            Tbc_Designer.SelectedIndex = 1;
        }

        private void Check_New_Job()
        {
            int row = 0, sl = 1;
            DateTime Todays = DateTime.Now.Date.AddDays(1);
            if (Dgv_New_Job.CurrentCell != null)
                row = Dgv_New_Job.CurrentCell.RowIndex;

            Dgv_New_Job.DataSource = null;
            Dgv_New_Job.Rows.Clear();
            _common.Dgv_Size(Dgv_New_Job, 11);

            var jobs = _db.New_Jobs.Where(x => x.Status == "Pro").OrderBy(x => x.Delivery).ToList();

            if (jobs.Find(x => x.Team == "Basic") != null)
            {
                Dgv_New_Job.Rows.Add("", "", "", "", "Basic Team Job Workload", "", "", "", "", "");
                foreach (var job in jobs)
                {
                    if (job.Team == "Basic")
                        Dgv_New_Job.Rows.Add(sl++, job.JobId, job.Folder, Math.Round(job.TargetTime, 1), job.Category, job.Service, job.InputAmount, job.ProDone, job.Delivery, job.Id);
                }
            }

            sl = 1;
            if (jobs.Find(x => x.Team == "Advance") != null)
            {
                Dgv_New_Job.Rows.Add("", "", "", "", "Advance Team Job Workload", "", "", "", "", "");
                foreach (var job in jobs)
                {
                    if (job.Team == "Advance")
                        Dgv_New_Job.Rows.Add(sl++, job.JobId, job.Folder, Math.Round(job.TargetTime, 1), job.Category, job.Service, job.InputAmount, job.ProDone, job.Delivery, job.Id);
                }
            }

            int max_row = Dgv_New_Job.Rows.Count - 1;
            if (max_row >= 0)
            {
                Dgv_New_Job.CurrentCell = Dgv_New_Job.Rows[max_row].Cells[3];
                if (max_row >= row)
                    Dgv_New_Job.CurrentCell = Dgv_New_Job.Rows[row].Cells[3];
            }

            _common.Row_Color_By_Delivery(Dgv_New_Job, "Column28");
        }

        private void Check_History()
        {
            DgvHistory.DataSource = null;
            DgvHistory.Rows.Clear();
            _common.Dgv_Size(DgvHistory, 11);

            var logs = _db.Logs
                .Where(x => x.Name == User.Short_Name)
                .OrderByDescending(x=>x.Id)
                .Take(999)
                .ToList();

            var sl = 1;
            foreach (var log in logs)
                DgvHistory.Rows.Add(sl++, log.JobId, log.Type, log.Image, log.Service, log.StartTime, log.EndTime, Math.Round(log.TargetTime, 1), Math.Round(log.ProTime, 2), log.Efficiency + "%");

            _common.Row_Color_By_Efficiency(DgvHistory, "Column10");
        }

        private void Check_Production_Error()
        {
            Dgv_Production_Error.DataSource = null;
            Dgv_Production_Error.Rows.Clear();
            _common.Dgv_Size(Dgv_Production_Error, 11);

            var errors = _db.Feedback
                .OrderByDescending(x => x.Id)
                .Take(999)
                .ToList();

            var sl = 1;
            foreach (var error in errors)
                Dgv_Production_Error.Rows.Add(sl++, error.Name, error.JobId, error.Folder, error.Image, error.Remarks, error.ReportTime, error.Reporter);

            //_common.Row_Color_By_Efficiency(Dgv_Production_Error, "Column55");
        }

        private void Check_Performance()
        {
            var date = _common.Shift_Date(DateTime.Now, _common.Current_Shift());
            Dgv_Performance.DataSource = null;
            Dgv_Performance.Rows.Clear();
            _common.Dgv_Size(Dgv_Performance, 11);

            var performances = (from per in _db.Performances
                                join user in _db.Users
                          on per.Name equals user.Short_Name
                          where per.Date == date & user.Role == ""
                          select new
                          {
                              per.Name,
                              per.Login,
                              per.Logout,
                              per.Shift,
                              per.WorkingTime,
                              per.File,
                              per.JobTime,
                              per.ProTime,
                              per.Efficiency,
                              per.Quality,
                          }).OrderByDescending(x => x.Efficiency).ToList();

            var sl = 1;
            foreach (var per in performances)
                Dgv_Performance.Rows.Add(sl++, per.Name, per.Shift, per.Login, per.Logout, per.WorkingTime, per.File, Math.Round(per.ProTime), per.Efficiency + "%", per.Quality + "%");

            _common.Row_Color_By_Efficiency(Dgv_Performance, "Column16");
            Check_Data();
        }

        private void Check_Done_Job()
        {
            Dgv_Done_Job.DataSource = null;
            Dgv_Done_Job.Rows.Clear();
            _common.Dgv_Size(Dgv_Done_Job, 11);

            var myJobs = (from my in _db.My_Jobs
                       join job in _db.New_Jobs 
                       on my.JobId equals job.JobId
                       where my.Name == User.Short_Name
                       select new {
                           my.Id,
                           Job_ID = my.JobId,
                           job.Folder,
                           my.Service,
                           my.Amount,
                           Job_Time = my.JobTime,
                           Pro_Time = my.ProTime,
                           Start_Time = my.StartTime,
                           End_Time = my.EndTime,
                           my.Efficiency,
                           my.Quality
                       }).OrderByDescending(x => x.Id).ToList();

            var sl = 1;
            foreach (var job in myJobs)
            {
                Dgv_Done_Job.Rows.Add(sl++, job.Job_ID, job.Folder, job.Service,  job.Amount, Math.Round(job.Job_Time, 1), Math.Round(job.Pro_Time, 2), job.Start_Time, job.End_Time, job.Efficiency+"%", job.Quality+"%");
            }

            _common.Row_Color_By_Efficiency(Dgv_Done_Job, "Column57");
        }

        private void Dashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            _common.Logout(Performance);
        }

        private void Dgv_New_Job_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (Dgv_New_Job.Columns[Dgv_New_Job.CurrentCell.ColumnIndex].HeaderText.Contains("Job_ID"))
            {
                var processing = Processing.GetInstance();
                if (Dgv_New_Job.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    processing._job.JobId = Dgv_New_Job.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    processing._runningJobsId = Convert.ToInt32(Dgv_New_Job.Rows[e.RowIndex].Cells[9].Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(Dgv_New_Job.CurrentCell.EditedFormattedValue.ToString()))
                {
                    Tmr_Count_Dashboard.Stop();
                    this.Hide();
                    processing.User = User;
                    processing._performance = Performance;
                    processing.Show();
                }
            }
        }

        private void Check_Data()
        {
            _db = new SkillContext();
            //check performance data
            var currentTime = DateTime.Now;
            var today = currentTime.Date;
            var start_time = _common.Shift_Time(User.Shift);
            if (User.Shift == "Night" & currentTime.ToString("tt").ToUpper() == "AM")
            {
                today = currentTime.AddDays(-1).Date;
                if(today != start_time.Date)
                    start_time = start_time.AddDays(-1);
            }

            Performance = _db.Performances
                .FirstOrDefault(x => x.Id == Performance.Id);

            Btn_Pro_Time.Text = @"Pro Time: " + Math.Round(Performance.ProTime);
            Btn_Efficiency.Text = @"Efficiency: " + Performance.Efficiency + @"%";

            if (start_time > currentTime)
                start_time = currentTime;

            var Capacity = (currentTime - start_time).TotalMinutes;
            Btn_Loss_Time.Text = @"Rest Time: " + Math.Round(Capacity - Performance.ProTime);
        }

        private void Tbc_Designer_SelectedIndexChanged(object sender, EventArgs e)
        {
            Tmr_Count_Dashboard.Start();
            switch (Tbc_Designer.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    Check_New_Job();
                    break;
                case 2:
                    Check_Performance();
                    break;
                case 3:
                    Check_History();
                    break;
                case 4:
                    Check_Production_Error();
                    break;
                case 5:
                    Check_Done_Job();
                    break;
            }
        }

        private void Dashboard_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                Check_Data();
                Tmr_Count_Dashboard.Start();
            }
        }

        private void Tmr_Count_Dashboard_Tick(object sender, EventArgs e)
        {
            Check_Data();
            if (Tbc_Designer.SelectedIndex == 1)
                Check_New_Job();
        }
    }
}