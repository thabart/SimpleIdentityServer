import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ScopeService } from '../../services';
import Table, { TableBody, TableCell, TableHead, TableRow, TableFooter, TablePagination } from 'material-ui/Table';
import { Popover, IconButton, Menu, MenuItem, Checkbox, TextField, Select, CircularProgress } from 'material-ui';
import MoreVert from '@material-ui/icons/MoreVert';
import Delete from '@material-ui/icons/Delete';
import Search from '@material-ui/icons/Search';

class ScopeComponent extends Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        this.handleClose = this.handleClose.bind(this);
        this.handleChangeProperty = this.handleChangeProperty.bind(this);
        this.handleChangeFilter = this.handleChangeFilter.bind(this);
        this.refreshData = this.refreshData.bind(this);
        this.handleChangePage = this.handleChangePage.bind(this);
        this.handleChangeRowsPage = this.handleChangeRowsPage.bind(this);
        this.handleRowClick = this.handleRowClick.bind(this);
        this.handleRemoveScopes = this.handleRemoveScopes.bind(this);
        this.handleAllSelections = this.handleAllSelections.bind(this);

        this.state = {
            data: [],
            isLoading: false,
            page: 1,
            pageSize: 5,
            count: 0,
            selectedType: 'all',
            selectedName: '',
            isSettingsOpened: false,
            isRemoveDisplayed: false,
            anchorEl: null,
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
    handleChangeProperty(e) {
        var self = this;
        self.setState({
            [e.target.name]: e.target.value
        });
    }

    /**
    * Handle the filter.
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
    * Display the data.
    */
    refreshData() {        
        var self = this;
        var startIndex = self.state.page * self.state.pageSize;
        const { t } = self.props;
        self.setState({
            isLoading: true
        });
        var request = { start_index: startIndex, count: self.state.pageSize };
        if (self.state.selectedName && self.state.selectedName !== '') {
            request['names'] = [ self.state.selectedName ];
        }

        if (self.state.selectedType !== 'all') {
            request['types'] = [ self.state.selectedType ];
        }

        ScopeService.search(request, self.props.type).then(function (result) {
            var data = [];
            if (result.content) {
                result.content.forEach(function (client) {
                    var type = client.type;
                    type = type === 1 ? t('apiScope') : t('openidScope');
                    data.push({
                        name: client['name'],
                        type: type,
                        isSelected: false
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
    * Remove the selected scopes.
    */
    handleRemoveScopes() {

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

    render() {
        var self = this;
        const { t } = this.props;
        var rows = [];
        if (self.state.data) {
            self.state.data.forEach(function(record) {
                rows.push((
                    <TableRow hover role="checkbox" key={record.name}>
                        <TableCell><Checkbox checked={record.isSelected} onChange={(e) => self.handleRowClick(e, record)} /></TableCell>
                        <TableCell>{record.name}</TableCell>
                        <TableCell>{record.type}</TableCell>
                    </TableRow>
                ));
            });
        }
        return (<div className="block">
            <div className="block-header">
                <h4>{t('scopes')}</h4>
                <i>{t('scopesDescription')}</i>
            </div>
            <div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('listOfScopes')}</h4>
                    <div style={{float: "right"}}>
                        {self.state.isRemoveDisplayed && (
                            <IconButton onClick={self.handleRemoveScopes}>
                                <Delete />
                            </IconButton>
                        )}
                        <IconButton onClick={this.handleClick}>
                            <MoreVert />
                        </IconButton>
                        <Menu anchorEl={self.state.anchorEl} open={Boolean(self.state.anchorEl)} onClose={self.handleClose}>
                            <MenuItem>{t('addScope')}</MenuItem>
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
                                        <TableCell>{t('name')}</TableCell>
                                        <TableCell>{t('scopeType')}</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    <TableRow>
                                        <TableCell><Checkbox onChange={self.handleAllSelections} /></TableCell>
                                        <TableCell>
                                            <form onSubmit={(e) => { e.preventDefault(); self.refreshData(); }}>
                                                <TextField value={this.state.selectedName} name='selectedName' onChange={self.handleChangeProperty} placeholder={t('Filter...')}/>
                                                <IconButton onClick={self.refreshData}><Search /></IconButton>
                                            </form>
                                        </TableCell>
                                        <TableCell>
                                            <Select value={this.state.selectedType} fullWidth={true} name="selectedType" onChange={this.handleChangeFilter}>
                                                <MenuItem value="all">{t('all')}</MenuItem>
                                                <MenuItem value="0">{t('openidScope')}</MenuItem>
                                                <MenuItem value="1">{t('apiScope')}</MenuItem>
                                            </Select>
                                        </TableCell>
                                    </TableRow>
                                    {rows}
                                </TableBody>
                                <TableFooter>
                                </TableFooter>
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

export default translate('common', { wait: process && !process.release })(ScopeComponent);