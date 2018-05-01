import React, { Component } from "react";
import { translate } from 'react-i18next';
import { NavLink } from 'react-router-dom';
import { Grid, Typography } from 'material-ui';

class Dashboard extends Component {
    render() {
        var self = this;
        const { t } = self.props;
        return (<div className="block">
            <div className="block-header">
                <Grid container>
                    <Grid item md={5} sm={12}>
                        <h4>{t('dashboardTitle')}</h4>
                        <i>{t('dashboardShortDescription')}</i>
                    </Grid>
                    <Grid item md={7} sm={12}>                        
                        <ul className="breadcrumb float-md-right">
                            <li className="breadcrumb-item"><NavLink to="/">{t('websiteTitle')}</NavLink></li>
                            <li className="breadcrumb-item">{t('dashboard')}</li>
                        </ul>
                    </Grid>
                </Grid>
            </div>
            <Grid container spacing={16}>
                <Grid item sm={12} md={4}>
                    <div className="card">
                        <div className="body">
                            <Typography variant="display1">10</Typography>
                            <Typography variant="caption" gutterBottom>{t('clients')}</Typography>
                        </div>
                    </div>
                </Grid>
                <Grid item sm={12} md={4}>
                    <div className="card">
                        <div className="body">
                            <Typography variant="display1">16</Typography>
                            <Typography variant="caption" gutterBottom>{t('scopes')}</Typography>
                        </div>
                    </div>
                </Grid>
                <Grid item sm={12} md={4}>
                    <div className="card">
                        <div className="body">
                            <Typography variant="display1">5</Typography>
                            <Typography variant="caption" gutterBottom>{t('resourceOwners')}</Typography>
                        </div>
                    </div>
                </Grid>
                <Grid item sm={12} md={6}>
                    <div className="card">
                        <div className="header">
                            <h4 style={{display: "inline-block"}}>{t('latestLogs')}</h4>
                        </div>
                        <div className="body">
                        </div>
                    </div>
                </Grid>
                <Grid item sm={12} md={6}>
                    <div className="card">
                        <div className="header">
                            <h4 style={{display: "inline-block"}}>{t('latestErrors')}</h4>
                        </div>
                        <div className="body">
                        </div>
                    </div>
                </Grid>
            </Grid>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(Dashboard);