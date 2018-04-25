import React, { Component } from "react";
import { translate } from 'react-i18next';
import Constants from '../constants';
import $ from 'jquery';
import moment from 'moment';

import { withStyles } from 'material-ui/styles';
import { CircularProgress } from 'material-ui';
import Input, { InputLabel } from 'material-ui/Input';
import { FormControl, FormHelperText } from 'material-ui/Form';

const styles = theme => ({
  margin: {
    margin: theme.spacing.unit,
  },
});

class ViewLog extends Component {
    constructor(props) {
        super(props);
        this.refreshData = this.refreshData.bind(this);
        this.state = {
            loading: false,
            id: '',
            aggregateId: '',
            description: '',
            createdOn: '',
            payload: ''
        };
    }

    refreshData() {
        var self = this;
        self.setState({
            loading: true
        });
        var href = Constants.eventSourceUrl + "/events/" + self.props.match.params.id;
        $.get(href).done(function (result) {
            self.setState({
                id: result.id,
                aggregateId: result.aggregate_id,
                description: result.description,
                createdOn: moment(result.createdOn).format('LLL'),
                payload: result.payload,
                loading: false
            });
        }).fail(function () {
            self.setState({
                loading: false
            });
        });

    }

    render() {
        var self = this;
        const { t, classes } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('logTitle')}</h4>
                <i>{t('logShortDescription')}</i>
            </div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-md-12">
                        <div className="card">
                            <div className="header"><h4>{t('logDescription')}</h4></div>
                            <div className="body">    
                                {this.state.loading ? ( <CircularProgress /> ) : (
                                    <div>       
                                        <FormControl fullWidth={true} className={classes.margin}>
                                            <InputLabel htmlFor="eventid">{t('eventId')}</InputLabel>
                                            <Input id="eventid" value={this.state.id}  />
                                        </FormControl>                                        
                                        <FormControl fullWidth={true} className={classes.margin}>                                    
                                            <InputLabel htmlFor="aggregateId">{t('aggregateId')}</InputLabel>
                                            <Input id="aggregateId" value={this.state.aggregateId}/>
                                        </FormControl>                                        
                                        <FormControl fullWidth={true} className={classes.margin}>                                    
                                            <InputLabel htmlFor="description">{t('eventDescription')}</InputLabel>
                                            <Input id="description" value={this.state.description}/>
                                        </FormControl>                                        
                                        <FormControl fullWidth={true} className={classes.margin}>                                    
                                            <InputLabel htmlFor="createdOn">{t('createdOn')}</InputLabel>
                                            <Input id="createdOn" value={this.state.createdOn}/>
                                        </FormControl>                                        
                                        <FormControl fullWidth={true} className={classes.margin}>                                    
                                            <InputLabel htmlFor="payload">{t('payload')}</InputLabel>
                                            <Input id="payload" multiline={true} value={this.state.payload}/>
                                        </FormControl>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        this.refreshData();
    }
}

export default translate('common', { wait: process && !process.release })(withStyles(styles)(ViewLog));