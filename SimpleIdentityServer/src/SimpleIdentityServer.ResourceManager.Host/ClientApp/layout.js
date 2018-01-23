import React, { Component } from "react";
import { NavLink } from "react-router-dom";

class Layout extends Component {
    render() {
        return (<div>
            <nav className="navbar-purple navbar-static-side">
                <div className="sidebar-collapse">
                    <ul className="nav flex-column">
                        <li className="nav-item"><NavLink to="/about" className="nav-link">About</NavLink></li>
                        <li className="nav-item"></li>
                        <li className="nav-item"></li>
                        <li className="nav-item active"></li>
                        <li className="nav-item"></li>
                        <li className="nav-item"></li>
                        <li className="nav-item"></li>
                        <li className="nav-item"><NavLink to="/login" className="nav-link"><span className="glyphicon glyphicon-user"></span> Login</NavLink></li>
                    </ul>
                </div>
            </nav>
            <section id="wrapper">
                { /* Navigation */ }
                <nav className="navbar navbar-toggleable-md navbar-light bg-light">
                    <a className="navbar-brand" href="#" id="uma-title">Uma</a>
                    <button type="button" className="navbar-toggler" data-toggle="collapse" data-target="#collapseNavBar">
                        <span className="navbar-toggler-icon"></span>
                    </button>
                    <div className="collapse navbar-collapse" id="collapseNavBar">
                        <ul className="navbar-nav mr-auto navbar-right">
                            <li className="nav-item"><NavLink to="/about" className="nav-link">About</NavLink></li>
                            <li className="nav-item"></li>
                            <li className="nav-item"></li>
                            <li className="nav-item active"></li>
                            <li className="nav-item"></li>
                            <li className="nav-item"></li>
                            <li className="nav-item"></li>
                            <li className="nav-item"><NavLink to="/login" className="nav-link"><span className="glyphicon glyphicon-user"></span> Login</NavLink></li>
                        </ul>
                    </div>
                </nav>
                { /* Display component */}
                <section id="body">
                    <div className="col-md-10 offset-md-1 cell">
                        {this.props.children}
                    </div>
                </section>
            </section>
        </div>);
    }
}


export default Layout;