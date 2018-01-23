import React, { Component } from "react";

class Login extends Component {
    render() {
        return (<form>
            <h4>Authenticate</h4>
            <div className="form-group">
                <input placeholder="Login" type="text" className="form-control" />
            </div>
            <div className="form-group">
                <input placeholder="Password" type="password" className="form-control" />
            </div>
            <button type="submit" className="btn btn-purple full-width">Login</button>
        </form>);
    }
}

export default Login;