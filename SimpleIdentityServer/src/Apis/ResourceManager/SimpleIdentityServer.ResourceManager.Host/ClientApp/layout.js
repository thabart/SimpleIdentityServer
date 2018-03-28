import React, { Component } from "react";
import { NavLink } from "react-router-dom";
import { SessionService } from './services';
import { withRouter } from 'react-router-dom';
import { translate } from 'react-i18next';
import AppDispatcher from './appDispatcher';
import Constants from './constants';

class Layout extends Component {
    constructor(props) {
        super(props);
        this._appDispatcher = null;
        this.disconnect = this.disconnect.bind(this);
        this.toggleMenu = this.toggleMenu.bind(this);
        this.state = {
            isLoggedIn: false,
            isOpenIdDisplayed: false,
            isScimDisplayed: false,
            isAuthDisplayed: false
        };
    }
    /**
     * Disconnect the user.
     * @param {any} e
     */
    disconnect(e) {
        e.preventDefault();
        SessionService.remove();
        this.setState({
            isLoggedIn: false
        });
        AppDispatcher.dispatch({
            actionName: Constants.events.USER_LOGGED_OUT
        });
        this.props.history.push('/');
    }

    toggleMenu(evt, menu) {  
        evt.preventDefault();      
        this.setState({
            [menu]: !this.state[menu]
        });
    }

    render() {
        var self = this;
        const { t } = this.props;
        return (<div>
            <nav className="navbar-static-side navbar left">
                <div className="sidebar-collapse">
                    <ul className="nav flex-column">
                        <li className="nav-item"><NavLink to="/about" className="nav-link">{t('aboutMenuItem')}</NavLink></li>
                        {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                            <li className="nav-item">
                                <a href="#" className="nav-link" onClick={(e) => self.toggleMenu(e, 'isOpenIdDisplayed')}>{t('manageOpenId')}</a>
                                {
                                    this.state.isOpenIdDisplayed && (
                                        <ul className="nav sub-nav flex-column">
                                            <li className="nav-item"><a href="#" className="nav-link">{t('openidClients')}</a></li>
                                            <li className="nav-item"><a href="#" className="nav-link">{t('openidScopes')}</a></li>
                                            <li className="nav-item"><a href="#" className="nav-link">{t('openidEndUsers')}</a></li>
                                            <li className="nav-item"><a href="#" className="nav-link">{t('openidConfigure')}</a></li>
                                        </ul>
                                    )
                                }
                            </li>
                        ))}
                        {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                            <li className="nav-item">
                                <a href="#" className="nav-link" onClick={(e) => self.toggleMenu(e, 'isScimDisplayed')}>{t('manageScim')}</a>
                                {
                                    this.state.isScimDisplayed && (
                                        <ul className="nav sub-nav flex-column">
                                            <li className="nav-item"><a href="#" className="nav-link">{t('scimSchemas')}</a></li> 
                                            <li className="nav-item"><a href="#" className="nav-link">{t('scimRessources')}</a></li>                                            
                                        </ul>
                                    )
                                }
                            </li>
                        ))}
                        {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                            <li className="nav-item">
                                <a href="#" className="nav-link" onClick={(e) => self.toggleMenu(e, 'isAuthDisplayed')}>{t('manageAuthServer')}</a>
                                {
                                    this.state.isAuthDisplayed && (
                                        <ul className="nav sub-nav flex-column">
                                            <li className="nav-item"><a href="#" className="nav-link">{t('scimSchemas')}</a></li> 
                                            <li className="nav-item"><a href="#" className="nav-link">{t('scimRessources')}</a></li>                                            
                                        </ul>
                                    )
                                }
                            </li>
                        ))}
                        {!process.env.IS_LOG_DISABLED && this.state.isLoggedIn  && (
                            <li className="nav-item"><NavLink to="/logs" className="nav-link">{t('logsMenuItem')}</NavLink></li>
                        )}
                        {(this.state.isLoggedIn ? (
                            <li className="nav-item"><a href="#" className="nav-link" onClick={this.disconnect}><span className="glyphicon glyphicon-user"></span> {t('disconnect')}</a></li>
                        ) : (
                                <li className="nav-item"><NavLink to="/login" className="nav-link"><span className="glyphicon glyphicon-user"></span> {t('connect')}</NavLink></li>
                        ))}
                    </ul>
                </div>
            </nav>
            <section id="wrapper">
                { /* Navigation */ }
                <nav className="navbar navbar-toggleable-md">
                    <a className="navbar-brand" href="#" id="uma-title">{t('websiteTitle')}</a>
                </nav>
                { /* Display component */}
                <section id="body">
                    {this.props.children}
                </section>
            </section>
        </div>);
    }
    componentDidMount() {
        var self = this;
        self.setState({
            isLoggedIn: !SessionService.isExpired()
        });
        self._appDispatcher = AppDispatcher.register(function (payload) {
            switch (payload.actionName) {
                case Constants.events.USER_LOGGED_IN:
                    self.setState({
                        isLoggedIn: true
                    });
                    break;
            }
        });
    }
    componentWillUnmount() {
        AppDispatcher.unregister(this._appDispatcher);
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Layout));