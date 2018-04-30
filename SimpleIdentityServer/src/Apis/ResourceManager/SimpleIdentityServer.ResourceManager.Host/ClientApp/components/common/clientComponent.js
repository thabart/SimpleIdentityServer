import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ClientService } from '../../services';
import { withRouter } from 'react-router-dom';

import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination } from 'material-ui/Table';
import { Popover, IconButton, Menu, MenuItem, Checkbox, TextField, Select, Avatar, CircularProgress } from 'material-ui';
import MoreVert from '@material-ui/icons/MoreVert';
import Delete from '@material-ui/icons/Delete';
import Search from '@material-ui/icons/Search';
import Visibility from '@material-ui/icons/Visibility';

class ClientComponent extends Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        this.handleClose = this.handleClose.bind(this);
        this.handleChangeValue = this.handleChangeValue.bind(this);
        this.handleChangeFilter = this.handleChangeFilter.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.handleChangePage = this.handleChangePage.bind(this);
        this.handleChangeRowsPage = this.handleChangeRowsPage.bind(this);
        this.handleRowClick = this.handleRowClick.bind(this);
        this.handleAllSelections = this.handleAllSelections.bind(this);
        this.handleRemoveClients = this.handleRemoveClients.bind(this);

        this.state = {
            data: [],
            isLoading: false,
            page: 0,
            pageSize: 5,
            count: 0,
            anchorEl: null,
            isRemoveDisplayed: false,
            selectedId: null,
            selectedType: 'all'
        };
    }

    /**
    * Open the menu.
    */
    handleClick(e) {
        this.setState({
            anchorEl: e.currentTarget
        });
    }

    /**
    * Close the menu.
    */
    handleClose() {
        this.setState({
            anchorEl: null
        });
    }

    /**
    * Handle the changes.
    */
    handleChangeValue(e) {
        if (!e.target) { return; }
        var self = this;
        self.setState({
            [e.target.name]: e.target.value
        });
    }

    /**
    * Handle the change
    */
    handleChangeFilter(e) {
        var self = this;
        self.setState({
            [e.target.name]: e.target.value
        }, () => {
            self.refreshData();
        });
    }

    /**
    * Refresh the clients.
    */
    refreshData() { 
        var self = this;
        var startIndex = self.state.page * self.state.pageSize;
        self.setState({
            isLoading: true
        });

        var request = { start_index: startIndex, count: self.state.pageSize };
        if (self.state.selectedId && self.state.selectedId !== '') {
            request['client_ids'] = [ self.state.selectedId ];
        }

        if (self.state.selectedType !== 'all') {
            request['client_types'] = [ self.state.selectedType ];
        }

        ClientService.search(request, self.props.type).then(function (result) {
            var data = [];
            if (result.content) {
                result.content.forEach(function (client) {
                    data.push({
                        logo_uri: client['logo_uri'],
                        client_name: client['client_name'],
                        client_id: client['client_id'],
                        isSelected: false,
                        type: client['application_type']
                    });
                });
            }
            
            self.setState({
                isLoading: false,
                data: data,
                count: result.count
            });
        }).catch(function (e) {
            self.setState({
                isLoading: false,
                data: [],
                count: 0
            });
        });
    }

    /**
    * Execute when the page has changed.
    */
    handleChangePage(evt, page) {
        var self = this;
        this.setState({
            page: page
        }, () => {
            self.refreshData();
        });
    }

    /**
    * Execute when the number of records has changed.
    */
    handleChangeRowsPage(evt) {
        var self = this;
        self.setState({
            pageSize: evt.target.value
        }, () => {
            self.refreshData();
        });
    }

    /**
    * Handle click on the row.
    */
    handleRowClick(e, record) {
        record.isSelected = e.target.checked;
        var data = this.state.data;
        var nbSelectedRecords = data.filter(function(r) { return r.isSelected; }).length;
        this.setState({
            data: data,
            isRemoveDisplayed: nbSelectedRecords > 0
        });
    }

    /**
    * Select / Unselect all scopes.
    */
    handleAllSelections(e) {
        var checked = e.target.checked;
        var data = this.state.data;
        data.forEach(function(r) { r.isSelected = checked ;});
        this.setState({
            data: data,
            isRemoveDisplayed: checked
        });
    }

    /**
    * Remove the selected clients.
    */
    handleRemoveClients() {      
    }


    render() {
        var self = this;
        const { t } = this.props;
        var rows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {
                rows.push((
                    <TableRow hover role="checkbox" key={record.client_id}>
                        <TableCell><Checkbox checked={record.isSelected} onChange={(e) => self.handleRowClick(e, record)} /></TableCell>
                        <TableCell><Avatar src={record.logo_uri}/></TableCell>
                        <TableCell>{record.client_name}</TableCell>
                        <TableCell>{record.client_id}</TableCell>
                        <TableCell>{record.type}</TableCell>
                        <TableCell>
                            <IconButton onClick={ () => self.props.history.push('/viewClient/' + self.props.type + '/' + record.client_id) }><Visibility /></IconButton>
                        </TableCell>
                    </TableRow>
                ));
            });
        }

        return (<div className="block">
            <div className="block-header">
                <h4>{t('oauthClients')}</h4>
                <i>{t('oauthClientsShortDescription')}</i>
            </div>
            <div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('listOfClients')}</h4>
                    <div style={{float: "right"}}>
                        {self.state.isRemoveDisplayed && (
                            <IconButton onClick={self.handleRemoveClients}>
                                <Delete />
                            </IconButton>
                        )}
                        <IconButton onClick={this.handleClick}>
                            <MoreVert />
                        </IconButton>
                        <Menu anchorEl={self.state.anchorEl} open={Boolean(self.state.anchorEl)} onClose={self.handleClose}>
                            <MenuItem>{t('addClient')}</MenuItem>
                        </Menu>
                    </div>
                </div>
                <div className="body">
                    { this.state.isLoading ? (<CircularProgress />) : (
                        <div>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell>{t('clientName')}</TableCell>
                                        <TableCell>{t('clientId')}</TableCell>
                                        <TableCell>{t('clientType')}</TableCell>
                                        <TableCell></TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    <TableRow>
                                        <TableCell><Checkbox onChange={self.handleAllSelections} /></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell></TableCell>
                                        <TableCell>
                                            <form onSubmit={(e) => { e.preventDefault(); self.refreshData(); }}>
                                                <TextField value={this.state.selectedId} name='selectedId' onChange={this.handleChangeValue} placeholder={t('Filter...')}/>
                                                <IconButton onClick={self.refreshData}><Search /></IconButton>
                                            </form>
                                        </TableCell>
                                        <TableCell>                                                                                               
                                            <Select value={this.state.selectedType} fullWidth={true} name="selectedType" onChange={this.handleChangeFilter}>
                                                <MenuItem value="all">{t('all')}</MenuItem>
                                                <MenuItem value="0">{t('native')}</MenuItem>
                                                <MenuItem value="1">{t('web')}</MenuItem>
                                            </Select>
                                        </TableCell>
                                        <TableCell></TableCell>
                                    </TableRow>
                                    {rows}
                                </TableBody>
                            </Table>
                            <TablePagination component="div" count={self.state.count} rowsPerPage={self.state.pageSize} page={this.state.page} onChangePage={self.handleChangePage} onChangeRowsPerPage={self.handleChangeRowsPage} />
                        </div>
                    )}
                </div>
            </div>
        </div>);
    }

    componentDidMount() {        
        this.refreshData();
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(ClientComponent));