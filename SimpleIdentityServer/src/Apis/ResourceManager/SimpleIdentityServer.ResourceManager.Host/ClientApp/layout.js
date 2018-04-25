import React, { Component } from "react";
import { NavLink } from "react-router-dom";
import { SessionService } from './services';
import { withRouter, Link } from 'react-router-dom';
import { translate } from 'react-i18next';
import { EndpointService } from './services';
import Constants from './constants';
import AppDispatcher from './appDispatcher';

import { IconButton, Button , Drawer, Select, MenuItem, SwipeableDrawer, FormControl } from 'material-ui';
import  List, { ListItem, ListItemText } from 'material-ui/List';
import { InputLabel } from 'material-ui/Input';
import { withStyles } from 'material-ui/styles';
import ExpandLess from '@material-ui/icons/ExpandLess';
import ExpandMore from '@material-ui/icons/ExpandMore';
import Settings from '@material-ui/icons/Settings';
import Collapse from 'material-ui/transitions/Collapse';


const drawerWidth = 300;
const styles = theme => ({
  root: {
    display: 'flex'
  },
  body: {
    width: '100%',
  },
  drawerPaper: {
    position: 'relative',
    width: drawerWidth,
  },
  formControl: {
    margin: theme.spacing.unit,
    minWidth: 120,
  }
});

class Layout extends Component {
    constructor(props) {
        super(props);
        this._appDispatcher = null;
        this._sessionFrame = null;
        this._checkSessionInterval = null;
        this.disconnect = this.disconnect.bind(this);
        this.toggleValue = this.toggleValue.bind(this);
        this.navigate = this.navigate.bind(this);
        this.refresh = this.refresh.bind(this);
        this.startCheckSession = this.startCheckSession.bind(this);
        this.stopCheckSession = this.stopCheckSession.bind(this);
        this.state = {
            isManageOpenidServerOpened: false,
            isManageAuthServersOpened: false,
            isScimOpened: false,
            isLoggedIn: false,
            isOauthDisplayed: false,
            isScimDisplayed: false,
            isAuthDisplayed: false,
            isDrawerDisplayed: false,
            openidEndpoints: [],
            authEndpoints: [],
            scimEndpoints: [],
            selectedOpenid: null,
            selectedAuth: null,
            selectedScim: null
        };
    }
    /**
     * Disconnect the user.
     * @param {any} e
     */
    disconnect() {
        // TODO : Resolve this url.
        var url = "http://localhost:60000/end_session?post_logout_redirect_uri=http://localhost:64950/end_session&id_token_hint="+ SessionService.getSession().id_token;
        var w = window.open(url, 'targetWindow', 'toolbar=no,location=no,status=no,menubar=no,scrollbars=yes,resizable=yes,width=400,height=400');
        var interval = setInterval(function() {
            if (w.closed) {
                clearInterval(interval);
                return;
            }

            var href = w.location.href;
            if (href === "http://localhost:64950/end_session") {                
                clearInterval(interval);
                w.close();
            }
        });
    }

    toggleValue(menu) {    
        this.setState({
            [menu]: !this.state[menu]
        });
    }

    navigate(href) {
        this.props.history.push(href);
    }

    /**
    * Refresh the settings menu.
    */
    refresh() {
        var self = this;
        EndpointService.getAll().then(function(endpoints) {
            var authEndpoints = endpoints.filter(function(endpoint) { return endpoint.type === 0; });
            var openidEndpoints = endpoints.filter(function(endpoint) { return endpoint.type === 1; });
            var scimEndpoints = endpoints.filter(function(endpoint) { return endpoint.type === 2; });
            var selectedOpenid = null;
            var selectedAuth = null;
            var selectedScim = null;
            if (authEndpoints.length > 0) {
                selectedAuth = authEndpoints[0].name;
            }

            if (openidEndpoints.length > 0) {
                selectedOpenid = openidEndpoints[0].name;
            }

            if (scimEndpoints.length > 0) {
                selectedScim = scimEndpoints[0].name;
            }

            self.setState({
                authEndpoints: authEndpoints,
                openidEndpoints: openidEndpoints,
                scimEndpoints: scimEndpoints,
                selectedOpenid: selectedOpenid,
                selectedAuth: selectedAuth,
                selectedScim: selectedScim
            });
        }).catch(function() {

        });
    }

    /**
    * Start the check the session.
    */
    startCheckSession() {
        var self = this;
        if (self._checkSessionInterval) {
            return;
        }

        var evt = window.addEventListener("message", function(e) {
            if (e.data !== 'unchanged') {
                SessionService.remove();
                console.log(e.data);
                self.setState({
                    isLoggedIn: false
                });
                AppDispatcher.dispatch({
                    actionName: Constants.events.USER_LOGGED_OUT
                });
                self.props.history.push('/');
            }
        }, false);
        var originUrl = window.location.protocol + "//" + window.location.host;
        self._checkSessionInterval = setInterval(function() { 
            var session = SessionService.getSession();
            var message = "ResourceManagerClientId ";
            if (session) {
                message += session.sessionState;
            } else {
                session += "tmp";
            }

            var win = self._sessionFrame.contentWindow;
            // TODO : Externalize the client_id & openid url.
            win.postMessage(message, "http://localhost:60000");
        }, 3*1000);
    }

    /**
    * Stop to check the session.
    */
    stopCheckSession() {
        var self = this;
        if (!self._checkSessionInterval) {
            return;
        }

        clearInterval(self._checkSessionInterval);
        self._checkSessionInterval = null;
    }

    render() {
        var self = this;
        const { t, classes } = this.props;
        var openidEndpoints = [];
        var authEndpoints = [];
        var scimEndpoints = [];
        if (self.state.openidEndpoints) {
            self.state.openidEndpoints.forEach(function(openidEndpoint) {
                openidEndpoints.push((<MenuItem key={openidEndpoint.name} value={openidEndpoint.name}>{openidEndpoint.description}</MenuItem>));
            });

            console.log(openidEndpoints);
            console.log(self.state.selectedOpenid);
        }

        if (self.state.authEndpoints) {
            self.state.authEndpoints.forEach(function(authEndpoint) {
                authEndpoints.push((<MenuItem key={authEndpoint.name} value={authEndpoint.name}>{authEndpoint.description}</MenuItem>));
            });
        }

        if (self.state.scimEndpoints) {
            self.state.scimEndpoints.forEach(function(scimEndpoint) {
                scimEndpoints.push((<MenuItem key={scimEndpoint.name} value={scimEndpoint.name}>{scimEndpoint.description}</MenuItem>))
            });
        }

        return (
        <div className={classes.root}>
            <SwipeableDrawer open={self.state.isDrawerDisplayed} anchor="right" onClose={ () => self.setState({ isDrawerDisplayed: false }) } onOpen={ () => self.setState({ isDrawerDisplayed: true }) }>
                <div style={{padding: "20px"}}>
                    <ul className="nav nav-tabs">
                        <li className="nav-item"><a href="#" className="nav-link">{t('yourPreferences')}</a></li>
                    </ul>
                    <div>
                        {openidEndpoints.length > 0 && (
                            <div>
                                <FormControl className={classes.formControl}>
                                    <InputLabel>{t('selectedPreferredOpenIdProvider')}</InputLabel>
                                    <Select value={self.state.selectedOpenid}>
                                        {openidEndpoints}
                                    </Select>
                                </FormControl>
                            </div>
                        )}
                        {authEndpoints.length > 0 && (
                            <div>
                                <FormControl className={classes.formControl}>
                                    <InputLabel>{t('selectPreferredAuthorizationServer')}</InputLabel>
                                    <Select value={self.state.selectedAuth}>
                                        {authEndpoints}
                                    </Select>
                                </FormControl>
                            </div>
                        )}
                        {scimEndpoints.length > 0 && (
                            <div>
                                <FormControl className={classes.formControl}>
                                    <InputLabel>{t('selectPreferredScimServer')}</InputLabel>                            
                                    <Select color="orange" value={self.state.selectedScim}>
                                        {scimEndpoints}
                                    </Select>
                                </FormControl>
                            </div>
                        )}
                        <Button color="primary">{t('saveChanges')}</Button>
                    </div>
                </div>
            </SwipeableDrawer>
            <Drawer variant="permanent" anchor="left" classes={{ paper: classes.drawerPaper }}>                
                <List>
                    {/* About menu item */}
                    <ListItem button onClick={() => self.navigate('/about')}>
                        <ListItemText>{t('aboutMenuItem')}</ListItemText>
                    </ListItem>
                    {/* Openid menu item */}                        
                    {(self.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <ListItem button onClick={() => self.toggleValue('isManageOpenidServerOpened')}>
                            <ListItemText>{t('manageOpenidServers')}</ListItemText>
                            { this.state.isManageOpenidServerOpened ? <ExpandLess /> : <ExpandMore /> }
                        </ListItem>
                    ))}
                    <Collapse in={this.state.isManageOpenidServerOpened}>
                        <List disablePadding>
                            <ListItem button onClick={() => self.navigate('/resourceowners')}><ListItemText>{t('resourceOwners')}</ListItemText></ListItem>
                            <ListItem button onClick={() => self.navigate('/openidclients')}><ListItemText>{t('openidclients')}</ListItemText></ListItem>
                            <ListItem button onClick={() => self.navigate('/openidscopes')}><ListItemText>{t('openidScopes')}</ListItemText></ListItem>
                        </List>
                    </Collapse>                        
                    {/* Authorisation server */}
                    {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <ListItem button onClick={() => self.toggleValue('isManageAuthServersOpened')}>
                            <ListItemText>{t('manageAuthServers')}</ListItemText>
                            { this.state.isManageAuthServersOpened ? <ExpandLess /> : <ExpandMore /> }
                        </ListItem>
                    ))}
                    <Collapse in={this.state.isManageAuthServersOpened}>
                        <List disablePadding>
                            <ListItem button onClick={() => self.navigate('/authclients')}><ListItemText>{t('oauthClients')}</ListItemText></ListItem>
                            <ListItem button onClick={() => self.navigate('/oauthScopes')}><ListItemText>{t('authscopes')}</ListItemText></ListItem>
                            <ListItem button onClick={() => self.navigate('/resources')}><ListItemText>{t('resources')}</ListItemText></ListItem>
                        </List>
                    </Collapse>
                    {/* SCIM server */}
                    {(this.state.isLoggedIn && !process.env.IS_MANAGE_DISABLED && (
                        <ListItem button onClick={() => self.toggleValue('isScimOpened')}>
                            <ListItemText>{t('manageScimServers')}</ListItemText>
                            { this.state.isScimOpened ? <ExpandLess /> : <ExpandMore /> }
                        </ListItem>
                    ))}
                    <Collapse in={this.state.isScimOpened}>
                        <List disablePadding>
                            <ListItem button><ListItemText>{t('scimSchemas')}</ListItemText></ListItem>
                            <ListItem button><ListItemText>{t('scimResources')}</ListItemText></ListItem>
                        </List>
                    </Collapse>
                    {/* Logs */}         
                    {!process.env.IS_LOG_DISABLED && this.state.isLoggedIn  && (
                       <ListItem button onClick={() => self.navigate('/logs')}>
                            <ListItemText>{t('logsMenuItem')}</ListItemText>
                        </ListItem>
                    )}
                    {/* Connect or disconnect */}
                    {(this.state.isLoggedIn ? (
                        <ListItem button onClick={() => self.disconnect()}>
                            <ListItemText>{t('disconnectMenuItem')}</ListItemText>
                        </ListItem>
                    ) : (                        
                        <ListItem button onClick={() => self.navigate('/login')}>
                            <ListItemText>{t('connectMenuItem')}</ListItemText>
                        </ListItem>
                    ))}
                </List>
            </Drawer>
            <section className={classes.body}>
                { /* Navigation */ }
                <nav className="navbar navbar-toggleable-md">
                    <a className="navbar-brand" href="#" id="uma-title">{t('websiteTitle')}</a>
                    <ul className="navbar-nav mr-auto">
                    </ul>
                    {this.state.isLoggedIn && (<IconButton  onClick={() => self.toggleValue('isDrawerDisplayed')}><Settings button  /></IconButton>)}
                </nav>
                { /* Display component */}
                <section id="body">
                    {this.props.children}
                </section>
            </section>
            { this.state.isLoggedIn && (<div>
                    <iframe ref={(elt) => { self._sessionFrame = elt; self.startCheckSession(); }} id="session-frame" src="http://localhost:60000/check_session" style={{display: "none"}} /> 
                </div>
            )}
        </div>);
    }

    componentDidMount() {
        var self = this;
        var isLoggedIn = !SessionService.isExpired();
        self.setState({
            isLoggedIn: isLoggedIn
        });
        self._appDispatcher = AppDispatcher.register(function (payload) {
            switch (payload.actionName) {
                case Constants.events.USER_LOGGED_IN:
                    self.setState({
                        isLoggedIn: true
                    });
                    self.refresh();
                    break;
            }
        });
        if (isLoggedIn) {
            self.refresh();
        }
    }

    componentWillUnmount() {
        AppDispatcher.unregister(this._appDispatcher);
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(withStyles(styles)(Layout)));