import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { NgxsModule } from '@ngxs/store';

// material modules
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AdminPortalComponent } from './admin-portal/admin-portal.component';
import { WordsState } from '../state/words/words.state';
import { WordListComponent } from './word-list/word-list.component';
import { WordEditComponent } from './word-list/word-edit/word-edit.component';
import { DefinitionEditComponent } from './word-list/word-edit/definition-edit/definition-edit.component';
import { ConfirmationPromptComponent } from './confirmation-prompt/confirmation-prompt.component';
import { UserListComponent } from './user-list/user-list.component';

const routes: Routes = [
  {
    path: '',
    component: AdminPortalComponent,
    children: [
      { path: 'words', title: 'OhMyWord - Words List', component: WordListComponent },
      { path: 'users', title: 'OhMyWord - Users List', component: UserListComponent },
    ],
  },
];

@NgModule({
  declarations: [
    AdminPortalComponent,
    WordListComponent,
    WordEditComponent,
    ConfirmationPromptComponent,
    UserListComponent,
    DefinitionEditComponent,
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    NgxsModule.forFeature([WordsState]),
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatMenuModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,
  ],
})
export class AdminModule {}
