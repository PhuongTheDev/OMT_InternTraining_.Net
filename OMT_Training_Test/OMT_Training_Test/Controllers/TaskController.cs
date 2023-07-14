﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMT_Training_Test.Models;
using OMT_Training_Test.Utility;
using System.Globalization;

namespace OMT_Training_Test.Controllers
{
    [Route("tasks")]
    [ApiController]
    public class TaskController : Controller
    {
        public IConfiguration _configuration;

        private readonly WorkManagementContext _context;
        public TaskController(WorkManagementContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> Tasks(string? keyWord = "", int? status = null, int? tag = 0, int page = 1)
        {
            var result = await _context.Tasks.ToListAsync();
            if (status == 99)
            {
                result = result.Where(o => (o.TaskName == keyWord) || (o.TaskDescription == keyWord) || keyWord == "")
               .Where(o => o.Status == 1 && DateTime.Compare(o.FinishDate, DateTime.Now) < 1)
               .Where(o => o.TagId == tag || tag == 0).ToList();
            }
            else
            {
                result = result.Where(o => (o.TaskName == keyWord) || (o.TaskDescription == keyWord) || keyWord == "")
                .Where(o => o.Status == status || status == null).Where(o => o.TagId == tag || tag == 0).ToList();
            }
            if (result.Count() < 10)
            {
                return Ok(new ResponseData<Models.Task>
                {
                    CurrentPage = page,
                    TotalPage = 1,
                    Data = result

                });
            }
            else if (result.Count() > 10)
            {
                return Ok(new ResponseData<Models.Task>
                {
                    CurrentPage = page,
                    TotalPage = 1,
                    Data = result.Skip(page - 1).Take(10).ToList()

                }); ;
            }
            return Ok(new ResponseData<Models.Task>
            {
                CurrentPage = 1,
                TotalPage = 0,
                Data = result

            }); ;
        }

        [HttpPost("add")]
        public ApiResponse<OMT_Training_Test.Models.Task> PostTask(DTO.TaskTest task)
        {

            try
            {
                List<string> errorMessage = new List<string>();
                bool validate = true;
                DateTime d;
                if (task.TaskName == "" || task.TaskDescription == "" || task.FinishDate == "")
                {
                    errorMessage.Add("Trường tên công việc, mô tả công việc và ngày hoàn thành là bắt buộc");
                    validate = false;
                }
                if (!RegexUtility.nameRegex.IsMatch(task?.TaskName))
                {
                    errorMessage.Add("Lỗi định dạng tên công việc");
                    validate = false;
                }
                if (!DateTime.TryParseExact(task?.FinishDate, RegexUtility.dateFormats[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                    && !DateTime.TryParseExact(task?.FinishDate, RegexUtility.dateFormats[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                {
                    errorMessage.Add("Lỗi định dạng ngày");
                    validate = false;
                }

                if (validate)
                {
                    var addedTask = new Models.Task
                    {
                        FinishDate = Convert.ToDateTime(task.FinishDate),
                        TaskDescription = task.TaskDescription,
                        TaskName = task.TaskName,
                        Status = 1,
                        TagId = task.TagId
                    };
                    _context.Tasks.Add(addedTask);
                    if (task.TagId != null)
                    {
                        var tag = _context.Tags.SingleOrDefault(o => o.TagId == task.TagId);
                        tag.OpenNumber += 1;
                    }
                    _context.SaveChanges();
                    return new ApiResponse<Models.Task>
                    {
                        Status = 1,
                        Data = addedTask
                    };
                }
                else
                {
                    return new ApiResponse<Models.Task>
                    {
                        Status = 0,
                        Message = errorMessage
                    };
                }
            }
            catch (Exception)
            {
                return new ApiResponse<Models.Task>
                {
                    Status = 0,
                    Message = new List<string> { "Có lỗi xảy ra" }
                };
            }
        }

        [HttpPut("update")]
        public ApiResponse<OMT_Training_Test.Models.Task> PutTask(DTO.TaskTest task)
        {
            try
            {
                List<string> errorMessage = new List<string>();
                var taskEdited = _context.Tasks.SingleOrDefault(o => o.TaskId == task.TaskId);
                if (taskEdited != null)
                {
                    bool validate = true;
                    DateTime d;
                    if (task.TaskName == "" || task.TaskDescription == "" || task.FinishDate == "")
                    {
                        errorMessage.Add("Trường tên công việc, mô tả công việc và ngày hoàn thành là bắt buộc");
                        validate = false;
                    }
                    if (!RegexUtility.nameRegex.IsMatch(task?.TaskName))
                    {
                        errorMessage.Add("Lỗi định dạng tên công việc");
                        validate = false;
                    }
                    if (!DateTime.TryParseExact(task?.FinishDate, RegexUtility.dateFormats[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                        && !DateTime.TryParseExact(task?.FinishDate, RegexUtility.dateFormats[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                    {
                        errorMessage.Add("Lỗi định dạng ngày");
                        validate = false;
                    }
                    if (validate)
                    {
                        taskEdited.Status = task.Status;
                        taskEdited.FinishDate = Convert.ToDateTime(task.FinishDate);
                        taskEdited.TaskDescription = task.TaskDescription;
                        taskEdited.TagId = task.TagId;
                        taskEdited.TaskName = task.TaskName;
                        _context.Update(taskEdited);
                        _context.SaveChanges();
                        return new ApiResponse<Models.Task>
                        {
                            Status = 1
                        };
                    }
                    else
                    {
                        return new ApiResponse<Models.Task>
                        {
                            Status = 0,
                            Message = errorMessage
                        };
                    }

                }
                else
                {
                    errorMessage.Add("Công việc không tồn tại");
                    return new ApiResponse<Models.Task>
                    {
                        Status = 0,
                        Message = errorMessage
                    };
                }
            }
            catch (Exception)
            {
                return new ApiResponse<Models.Task>
                {
                    Status = 0,
                    Message = new List<string> { "Có lỗi xảy ra" }
                };
            }
        }

        [HttpDelete("delete")]
        public ApiResponse<OMT_Training_Test.Models.Task> DeleteTask(int taskId)
        {
            try
            {
                var task = _context.Tasks.SingleOrDefault(o => o.TaskId == taskId);
                if (task != null)
                {
                    if (task.TagId != null)
                    {
                        var tag = _context.Tags.SingleOrDefault(o => o.TagId == task.TagId);
                        if (task.Status == 1)
                        {
                            tag.OpenNumber -= 1;
                        }
                        else if (task.Status == 2)
                        {
                            tag.FinishNumber -= 1;
                        }
                        else if (task.Status == 3)
                        {
                            tag.CloseNumber -= 1;
                        }
                    }
                    _context.Remove(task);
                    _context.SaveChanges();
                    return new ApiResponse<Models.Task>
                    {
                        Status = 1
                    };
                }
                else
                {
                    return new ApiResponse<Models.Task>
                    {
                        Status = 0,
                        Message = new List<string> { "Bạn không thể xóa công việc này" }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<Models.Task>
                {
                    Status = 0,
                    Message = new List<string> { "Có lỗi xảy ra", ex.ToString() }
                };
            }
        }

        [HttpPut("done")]
        public ApiResponse<OMT_Training_Test.Models.Task> FinishTask(int taskId)
        {
            try
            {
                var a = _context.Tasks.SingleOrDefault(o => o.TaskId == taskId);
                if (a != null)
                {
                    if (a.TagId != null)
                    {
                        var tag = _context.Tags.SingleOrDefault(o => o.TagId == a.TagId);
                        if (a.Status == 1)
                        {
                            tag.OpenNumber -= 1;
                            tag.FinishNumber += 1;
                        }
                        else if (a.Status == 3)
                        {
                            tag.CloseNumber -= 1;
                            tag.FinishNumber += 1;
                        }
                    }
                    a.Status = 2;
                    _context.SaveChanges();
                    return new ApiResponse<Models.Task>
                    {
                        Status = 1
                    };
                }
                else
                {
                    return new ApiResponse<Models.Task>
                    {
                        Status = 0,
                        Message = new List<string> { "Bạn không thể hoàn thành công việc này" }
                    };
                }

            }
            catch (Exception ex)
            {
                return new ApiResponse<Models.Task>
                {
                    Status = 0,
                    Message = new List<string> { "Có lỗi xảy ra", ex.ToString() }
                };
            }
        }


        [HttpPut("close")]
        public ApiResponse<OMT_Training_Test.Models.Task> CloseTask(int taskId)
        {
            try
            {
                var a = _context.Tasks.SingleOrDefault(o => o.TaskId == taskId);
                if (a != null)
                {
                    if (a.TagId != null)
                    {
                        var tag = _context.Tags.SingleOrDefault(o => o.TagId == a.TagId);
                        if (a.Status == 1)
                        {
                            tag.OpenNumber -= 1;
                            tag.CloseNumber += 1;
                        }
                        else if (a.Status == 2)
                        {
                            tag.FinishNumber -= 1;
                            tag.CloseNumber += 1;
                        }
                    }
                    a.Status = 3;
                    _context.SaveChanges();
                    return new ApiResponse<Models.Task>
                    {
                        Status = 1
                    };
                }
                else
                {
                    return new ApiResponse<Models.Task>
                    {
                        Status = 0,
                        Message = new List<string> { "Bạn không thể đóng công việc này" }
                    };
                }

            }
            catch (Exception ex)
            {
                return new ApiResponse<Models.Task>
                {
                    Status = 0,
                    Message = new List<string> { "Có lỗi xảy ra", ex.ToString() }
                };
            }
        }

        [HttpPut("reopen")]
        public ApiResponse<OMT_Training_Test.Models.Task> ReopenTask(int taskId)
        {
            try
            {
                var a = _context.Tasks.SingleOrDefault(o => o.TaskId == taskId);
                if (a != null)
                {
                    if (a.TagId != null)
                    {
                        var tag = _context.Tags.SingleOrDefault(o => o.TagId == a.TagId);
                        if (a.Status == 2)
                        {
                            tag.FinishNumber -= 1;
                            tag.OpenNumber += 1;
                        }
                        else if (a.Status == 3)
                        {
                            tag.CloseNumber -= 1;
                            tag.OpenNumber += 1;
                        }
                    }
                    a.Status = 1;
                    _context.SaveChanges();
                    return new ApiResponse<Models.Task>
                    {
                        Status = 1
                    };
                }
                else
                {
                    return new ApiResponse<Models.Task>
                    {
                        Status = 0,
                        Message = new List<string> { "Bạn không thể mở lại công việc này" }
                    };
                }

            }
            catch (Exception ex)
            {
                return new ApiResponse<Models.Task>
                {
                    Status = 0,
                    Message = new List<string> { "Có lỗi xảy ra", ex.ToString() }
                };
            }
        }

        [HttpGet("Tags")]
        public async Task<IActionResult> Tags()
        {
            var result = _context.Tags.ToList();
            return Ok(result);
        }
    }
}