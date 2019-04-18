using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Http;
using Data;
using System.Web.Http.Cors;
using PagedList;
using PagedList.Mvc;
using Newtonsoft.Json.Linq;

namespace SaloonShop.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
   
    public class EmployeeController : ApiController
    {
        private SaloonShopEntities Saloon { get; set; }
        public EmployeeController()
        {
            this.Saloon = new SaloonShopEntities();
        }

        
        //api/AddEmployee
        [Route("api/AddOrEditEmployee")]
        [HttpPost]
        public IHttpActionResult AddEditEmployee(Employee emp)
        {
            try
            {
                var newToken = Request.Headers.GetValues("Token").FirstOrDefault().ToString();
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    //HttpContext.Current.Response.Headers.Remove("Token");
                    //HttpContext.Current.Response.Headers.Add("Token",RefreshToken(newToken));
                    

                    if (emp.Id==0)
                    {   
                        Saloon.Employees.Add(emp);
                        Saloon.SaveChanges();
                        return Ok("Employee Added");
                    }
                    else
                    {
                        Saloon.Employees.Remove(Saloon.Employees.FirstOrDefault(x => x.Id==emp.Id));
                        Saloon.Employees.Add(emp);
                        Saloon.SaveChanges();
                        return Ok("Employee Updated");
                    }
                }
                else
                {
                    return Unauthorized();
                }             

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Route("api/RemoveEmployee/{empId}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult RemoveEmployee(int empId)
        {

            try
            {
                var newToken = Request.Headers.GetValues("Token").FirstOrDefault().ToString();
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    //HttpContext.Current.Response.Headers.Remove("Token");
                    //HttpContext.Current.Response.Headers.Add("Token", RefreshToken(newToken.ToString()));
                    Saloon.Employees.Remove(Saloon.Employees.Find((object)empId));
                    Saloon.SaveChanges();
                    return Json("Employee Removed");
                }
                else
                {
                    return Unauthorized();
                }
                   
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Route("api/ShowAllEmployees/{PageIndex}/{PageItems}")]
        [AcceptVerbs("GET")]
        public IHttpActionResult ShowAllEmployees(int PageIndex,int PageItems)
        {
            try
            {
                //var emp = Saloon.Employees.Where(x => x.IsArchived != true).Select(x=> new { x.Id,x.FirstName,x.ContactNumber,x.isAuthenticated,x.LastName,x.Password,x.UserName,x.authenticationkey}).ToList();
                var emp = Saloon.Employees.Select(x => new { x.Id, x.FirstName, x.ContactNumber, x.isAuthenticated, x.LastName, x.Password, x.UserName, x.authenticationkey }).ToList();
                var emps = emp.ToPagedList(PageIndex, PageItems);
                return Json(emps);
            }
            catch (Exception)
            {
                    
                throw;
            }
        }

        [Route("api/AuthEmployees")]
        [AcceptVerbs("POST")]
        public IHttpActionResult Login(string userName, string password)
        {
            try
            {
                Employee employee = Saloon.Employees.FirstOrDefault(x => x.UserName.Equals(userName) && x.Password.Equals(password));
                if (employee != null && !(bool) employee.isAuthenticated)
                {

                    employee.isAuthenticated = true;
                    employee.authenticationkey = Guid.NewGuid().ToString().Replace("-", "");
                    HttpContext.Current.Response.Headers.Add("X-Token", employee.authenticationkey);
                    Saloon.SaveChanges();
                    employee.Password = "";
                    return Json(employee.authenticationkey);
                }
                else if(employee != null && (bool) employee.isAuthenticated)
                {
                    employee.authenticationkey = RefreshToken(employee.authenticationkey);
                    Saloon.SaveChanges();
                    HttpContext.Current.Response.Headers.Add("X-Token", employee.authenticationkey);
                    return Json(employee.authenticationkey);
                }
                else
                {
                    return InternalServerError();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Route("api/RemoveAuthEmployees")]
        [AcceptVerbs("GET")]
        public IHttpActionResult Logout()
        {
            try
            {
                var newToken = Request.Headers.GetValues("Token").FirstOrDefault().ToString();
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    Employee employee = Saloon.Employees.FirstOrDefault(x => x.authenticationkey.Equals(newToken));
                    if (employee != null && (bool)employee.isAuthenticated)
                    {
                        employee.isAuthenticated = false;
                        employee.authenticationkey = null;
                        Saloon.SaveChanges();
                        return Ok("Session Removed Successfully");
                    }
                    else
                    {
                        return InternalServerError();
                    }
                }
                else
                {
                    return Unauthorized();
                }
                
            }
            catch (Exception)
            {
                 throw;
            }
           
        }
        [Route("api/getEmployeeById/{Id}")]
        [HttpGet]
        public IHttpActionResult getByID(int Id)
        {
            try
            {
                var newToken = Request.Headers.GetValues("Token").FirstOrDefault().ToString();
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    Employee emp = Saloon.Employees.Where(x => x.Id.Equals(Id)).FirstOrDefault();
                    emp.authenticationkey = RefreshToken(emp.authenticationkey);
                    return Json(emp);
                }
                else
                {
                    return Unauthorized();
                }
                
            }
            catch (Exception)
            {
                    
                throw;
            }
           
        }
        [Route("api/getPaginInfo")]
        [HttpGet]
        public IHttpActionResult getPaginInfo()
        {
            int PageNumber = 1;
            int pageSize = 10;
            int PageItems = Saloon.Employees.Count();
            return Json(Convert.ToString(PageNumber + ","+pageSize+","+PageItems));
        }

        public string RefreshToken(string token)
        {
            try
            {
                string newToken = Guid.NewGuid().ToString().Replace("-", "");
                Employee emp = Saloon.Employees.Where(x => x.authenticationkey.Equals(token)).FirstOrDefault();
                emp.authenticationkey = newToken;
                Saloon.SaveChanges();
                return token;
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }
    }
}
